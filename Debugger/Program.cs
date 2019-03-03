using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Debugger
{
    class Program
    {
        static void DownloadHttp(string Url, string Path)
        {
            HttpWebResponse response = (HttpWebResponse)WebRequest.Create(Url).GetResponse();
            if (response != null && response.StatusCode == HttpStatusCode.OK)
            using (var client = new WebClient())
            {
                client.DownloadFile(Url, Path);
            }
        }

        static void Unzip(string Archive, string File)
        {
            using (ZipArchive archive = ZipFile.OpenRead(Archive))
            {
                for (int i = 0; i < archive.Entries.Count; ++i)
                {
                    if (archive.Entries[i].FullName == File)
                    {
                        archive.Entries[i].ExtractToFile(archive.Entries[i].FullName);
                        break;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            string UEFI_EXT, QEMU_ARCH, FW_BASE, QEMU_OPTS;
            UEFI_EXT = QEMU_ARCH = FW_BASE = QEMU_OPTS = string.Empty;
            switch (args[0])
            {
                case "x86":
                    UEFI_EXT = @"ia32";
                    QEMU_ARCH = @"i386";
                    FW_BASE = @"OVMF";
                    break;
                case "x64":
                    UEFI_EXT = @"x64";
                    QEMU_ARCH = @"x86_64";
                    FW_BASE = @"OVMF";
                    break;
                case "ARM":
                    UEFI_EXT = @"arm";
                    QEMU_ARCH = @"arm";
                    FW_BASE = @"QEMU_EFI";
                    QEMU_OPTS = @"-M virt -cpu cortex-a15 ";
                    break;
                case "ARM64":
                    UEFI_EXT = @"aa64";
                    QEMU_ARCH = @"aarch64";
                    FW_BASE = @"QEMU_EFI";
                    QEMU_OPTS = @"-M virt -cpu cortex-a57 ";
                    break;
                default:
                    Console.WriteLine("Unsupported debug target: " + args[0]);
                    break;
            }

            string QEMU_PATH = @"C:\Program Files\qemu\";
            QEMU_OPTS += @"-net none -monitor none -parallel none";

            string BOOT_NAME = @"boot" + UEFI_EXT + @".efi";
            string QEMU_EXE = @"qemu-system-" + QEMU_ARCH + @"w.exe";

            string FW_ARCH = UEFI_EXT.ToUpper();
            string FW_DIR = @"https://efi.akeo.ie/" + FW_BASE + @"/";
            string FW_ZIP = FW_BASE + @"-" + FW_ARCH + @".zip";
            string FW_FILE = FW_BASE + @"_" + FW_ARCH + @".fd";
            string FW_URL = FW_DIR + FW_ZIP;

            if (!File.Exists(QEMU_PATH + QEMU_EXE))
            {
                Console.WriteLine("'" + QEMU_PATH + QEMU_EXE + "' was not found.\nPlease make sure QEMU is installed or edit the path in Program.cs");
                return;
            }

            if (!File.Exists(FW_FILE))
            {
                Console.WriteLine("The UEFI firmware file, needed for QEMU, will be downloaded from: " + FW_URL + "\n\nNote: Unless you delete the file, this should only happen once.");
                DownloadHttp(FW_URL, FW_ZIP);
            }

            if (!File.Exists(FW_ZIP) && !File.Exists(FW_FILE))
            {
                Console.WriteLine("There was a problem downloading the QEMU UEFI firmware.");
                return;
            }

            if (File.Exists(FW_ZIP))
            {
                Unzip(FW_ZIP, FW_BASE + ".fd");
                File.Move(FW_BASE + ".fd", FW_FILE);
                File.Delete(FW_ZIP);
            }

            if (!File.Exists(FW_FILE))
            {
                Console.WriteLine("There was a problem unzipping the QEMU UEFI firmware.");
                return;
            }

            Directory.CreateDirectory(@"image\efi\boot");

            File.Copy(args[1], "image\\efi\\boot\\" + BOOT_NAME, true);

            ProcessStartInfo process = new ProcessStartInfo();
            process.FileName = QEMU_PATH + QEMU_EXE;
            process.Arguments = QEMU_OPTS + " -L . -bios " + FW_FILE + " -hda fat:rw:image";
            Process.Start(process);
        }
    }
}
