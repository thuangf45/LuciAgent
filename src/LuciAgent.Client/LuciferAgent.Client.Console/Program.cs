using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    // --- P/Invoke definitions cho Windows WlanApi ---
    [DllImport("wlanapi.dll", SetLastError = true)]
    private static extern uint WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, out uint pdwNegotiatedVersion, out IntPtr phClientHandle);

    [DllImport("wlanapi.dll", SetLastError = true)]
    private static extern uint WlanCloseHandle(IntPtr hClientHandle, IntPtr pReserved);

    [DllImport("wlanapi.dll", SetLastError = true)]
    private static extern uint WlanEnumInterfaces(IntPtr hClientHandle, IntPtr pReserved, out IntPtr ppInterfaceList);

    [DllImport("wlanapi.dll", SetLastError = true)]
    private static extern uint WlanGetAvailableNetworkList(IntPtr hClientHandle, ref Guid pInterfaceGuid, uint dwFlags, IntPtr pReserved, out IntPtr ppAvailableNetworkList);

    [DllImport("wlanapi.dll", SetLastError = true)]
    private static extern void WlanFreeMemory(IntPtr pMemory);

    [StructLayout(LayoutKind.Sequential)]
    private struct WLAN_INTERFACE_INFO_LIST
    {
        public uint dwNumberOfItems;
        public uint dwIndex;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WLAN_INTERFACE_INFO
    {
        public Guid InterfaceGuid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strInterfaceDescription;
        public uint isState;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WLAN_AVAILABLE_NETWORK_LIST
    {
        public uint dwNumberOfItems;
        public uint dwIndex;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WLAN_AVAILABLE_NETWORK
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strProfileName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] dot11Ssid;
        public uint dot11BSSType;
        public uint numberOfBssids;
        public bool networkConnectable;
        public uint wlanNotConnectableReason;
        public uint uDot11PhyTypes;
        public uint bSecurityEnabled;
        public uint dot11DefaultAuthAlgorithm;
        public uint dot11DefaultCipherAlgorithm;
        public uint dwFlags;
        public uint dwReserved;
        public uint wlanSignalQuality;
    }

    static void Main()
    {
        // Ép Console hiển thị đúng Tiếng Việt UTF-8
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("==============================================");
        Console.WriteLine("   LUCIAGENT - MODULE QUÉT PHẦN CỨNG MẠNG");
        Console.WriteLine("==============================================\n");

        ScanWiFiNetworks();

        // Ví dụ cơ chế thử kết nối/password (nếu muốn test trên mạng của bạn)
        TestConnectWithPassword("Quynh", "14042000");

        Console.WriteLine("\nHoàn tất quá trình kiểm tra.");
    }

    static void ScanWiFiNetworks()
    {
        Console.WriteLine("Đang quét danh sách Wi-Fi xung quanh...");

        uint clientVersion = 2;
        uint negotiatedVersion;
        IntPtr clientHandle = IntPtr.Zero;

        uint result = WlanOpenHandle(clientVersion, IntPtr.Zero, out negotiatedVersion, out clientHandle);
        if (result != 0)
        {
            Console.WriteLine($"Lỗi mở WLAN handle: {result}");
            return;
        }

        try
        {
            IntPtr ppInterfaceList = IntPtr.Zero;
            result = WlanEnumInterfaces(clientHandle, IntPtr.Zero, out ppInterfaceList);
            if (result != 0)
            {
                Console.WriteLine($"Không thể liệt kê card mạng: {result}");
                return;
            }

            WLAN_INTERFACE_INFO_LIST header = Marshal.PtrToStructure<WLAN_INTERFACE_INFO_LIST>(ppInterfaceList);
            long pointerValue = ppInterfaceList.ToInt64() + 8;

            for (int i = 0; i < header.dwNumberOfItems; i++)
            {
                WLAN_INTERFACE_INFO info = Marshal.PtrToStructure<WLAN_INTERFACE_INFO>(new IntPtr(pointerValue));
                Console.WriteLine($"\n[Card Mạng]: {info.strInterfaceDescription}");

                IntPtr ppAvailableNetworkList = IntPtr.Zero;
                uint resNet = WlanGetAvailableNetworkList(clientHandle, ref info.InterfaceGuid, 0, IntPtr.Zero, out ppAvailableNetworkList);

                if (resNet == 0)
                {
                    WLAN_AVAILABLE_NETWORK_LIST netHeader = Marshal.PtrToStructure<WLAN_AVAILABLE_NETWORK_LIST>(ppAvailableNetworkList);
                    long netPointerValue = ppAvailableNetworkList.ToInt64() + 8;

                    for (int j = 0; j < netHeader.dwNumberOfItems; j++)
                    {
                        WLAN_AVAILABLE_NETWORK network = Marshal.PtrToStructure<WLAN_AVAILABLE_NETWORK>(new IntPtr(netPointerValue));

                        string ssid = Encoding.UTF8.GetString(network.dot11Ssid, 1, network.dot11Ssid.Length - 1).TrimEnd('\0');
                        if (string.IsNullOrEmpty(ssid))
                        {
                            ssid = "[Ẩn SSID / Hidden Network]";
                        }

                        Console.WriteLine($" - SSID: {ssid,-30} | Sóng (RSSI): {network.wlanSignalQuality}% | Bảo mật: {(network.bSecurityEnabled == 1 ? "Có" : "Mở")}");

                        netPointerValue += Marshal.SizeOf(typeof(WLAN_AVAILABLE_NETWORK));
                    }

                    WlanFreeMemory(ppAvailableNetworkList);
                }

                pointerValue += Marshal.SizeOf(typeof(WLAN_INTERFACE_INFO));
            }

            WlanFreeMemory(ppInterfaceList);
        }
        finally
        {
            WlanCloseHandle(clientHandle, IntPtr.Zero);
        }
    }

    /// <summary>
    /// Cơ chế thử kết nối vào Wi-Fi bằng mật khẩu (Dùng netsh của Windows ẩn sau tiến trình)
    /// </summary>
    static void TestConnectWithPassword(string ssid, string password)
    {
        Console.WriteLine($"\nĐang thử kết nối vào mạng [{ssid}] với mật khẩu...");

        // Bước 1: Tạo XML Profile tạm thời cho Windows
        string profileXml = $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{ssid}</name>
    <SSIDConfig>
        <SSID>
            <name>{ssid}</name>
        </SSID>
    </SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>auto</connectionMode>
    <MSM>
        <security>
            <authEncryption>
                <authentication>WPA2PSK</authentication>
                <encryption>AES</encryption>
                <useOneX>false</useOneX>
            </authEncryption>
            <sharedKey>
                <keyType>passPhrase</keyType>
                <protected>false</protected>
                <key>{password}</key>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";

        string tempFile = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(tempFile, profileXml, Encoding.UTF8);

        try
        {
            // Bước 2: Add profile vào Windows bằng netsh
            RunCommand($"netsh wlan add profile filename=\"{tempFile}\"");

            // Bước 3: Ra lệnh kết nối
            RunCommand($"netsh wlan connect name=\"{ssid}\"");

            Console.WriteLine("Đã gửi lệnh kết nối. Đang chờ phản hồi từ card mạng...");
            Thread.Sleep(3000); // Chờ 3 giây để hệ thống bắt tay mạng
        }
        finally
        {
            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);
            }
        }
    }

    static void RunCommand(string command)
    {
        var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (var process = Process.Start(processInfo))
        {
            process.WaitForExit();
        }
    }
}