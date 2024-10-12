using System.Diagnostics;

namespace ArchiveMaster.Views;

public class FileProgressDialog : ProgressDialog
{
    private CancellationTokenSource cts;

    private string destinationPath;

    private string sourcePath;

    public FileProgressDialog()
    {
        Title = "正在导出";
        SecondaryButtonContent = "中断";
    }
    public async Task CopyFileAsync(string sourcePath, string destinationPath, int bufferSize = 128 * 1024)
    {
        this.sourcePath = sourcePath;
        this.destinationPath = destinationPath;
        Message = "正在复制文件";
        cts = new CancellationTokenSource();

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        try
        {
            await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            await using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            long totalBytes = sourceStream.Length;
            Maximum = totalBytes;
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                cts.Token.ThrowIfCancellationRequested();
                await destinationStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                // Calculate progress percentage and report it.
                Value = totalBytesRead;
#if DEBUG
                await Task.Delay(100);
#endif
            }

            PrimaryButtonContent = "打开文件";
            CloseButtonContent = "完成";
            SecondaryButtonContent = "";
            Message = "完成";
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
            }
            catch
            {
            }

            Close();
        }
        catch (Exception ex)
        {
            Title = "错误";
            Message = ex.Message;
            CloseButtonContent = "取消";
            SecondaryButtonContent = "";
        }
    }

    protected override void OnCloseButtonClick()
    {
        Close();
    }

    protected override void OnPrimaryButtonClick()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = destinationPath,
            UseShellExecute = true
        });
        Close();
    }
    protected override void OnSecondaryButtonClick()
    {
        CloseButtonContent = false;
        cts?.Cancel();
    }
}