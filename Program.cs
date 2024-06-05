using System;
using System.IO;
using System.Net;

class FTPClient
{
    private string ftpServer;
    private string ftpUsername;
    private string ftpPassword;

    public FTPClient(string server, string username, string password)
    {
        ftpServer = server;
        ftpUsername = username;
        ftpPassword = password;
    }

    public void UploadDirectory(string localDirectory, string remoteDirectory)
    {
        try
        {
            // Ensure the local directory exists
            if (!Directory.Exists(localDirectory))
            {
                Console.WriteLine("Local directory does not exist.");
                return;
            }

            // Upload files in the local directory
            string[] files = Directory.GetFiles(localDirectory);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string remoteFilePath = remoteDirectory + "/" + fileName;
                UploadFile(file, remoteFilePath);
            }

            // Recursively upload subdirectories
            string[] directories = Directory.GetDirectories(localDirectory);
            foreach (string directory in directories)
            {
                string dirName = Path.GetFileName(directory);
                string remoteSubDirectory = remoteDirectory + "/" + dirName;

                // Create the directory on the FTP server
                CreateRemoteDirectory(remoteSubDirectory);

                // Recursively upload the contents of the subdirectory
                UploadDirectory(directory, remoteSubDirectory);
            }

            Console.WriteLine("All files and directories have been uploaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void UploadFile(string localFilePath, string remoteFilePath)
    {
        try
        {
            string uri = ftpServer + remoteFilePath;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            // Read the file data
            byte[] fileContents = File.ReadAllBytes(localFilePath);

            // Write the file data to the request stream
            request.ContentLength = fileContents.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            // Get the response
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void CreateRemoteDirectory(string remoteDirectory)
    {
        try
        {
            string uri = ftpServer + remoteDirectory;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Create Directory Complete, status {response.StatusDescription}");
            }
        }
        catch (WebException ex)
        {
            FtpWebResponse response = (FtpWebResponse)ex.Response;
            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                // Directory already exists, ignore the error
                Console.WriteLine("Directory already exists.");
            }
            else
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static void Main(string[] args)
    {
        string ftpServer = "ftp://<someserver>.in";
        string ftpUsername = "<someusername>";
        string ftpPassword = "SomeReallyStrongPassworrd";


        FTPClient client = new FTPClient(ftpServer, ftpUsername, ftpPassword);

        string localDirectory = @"C:\Users\someuser\source\repos\Almondcove\Almondcove.Web\bin\Release\net8.0\publish\";
        string remoteDirectory = "/httpdocs";

        client.UploadDirectory(localDirectory, remoteDirectory);
    }
}
