using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace resizeImage
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile _inputFile;
        public MainPage()
        {
            this.InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            createFile();

            //var result =  OpenDir();

        }

        private async Task createFile() {
            // Create sample file; replace if exists.
            FolderPicker p = new FolderPicker();
            p.FileTypeFilter.Add(".jpg");
            p.FileTypeFilter.Add(".png");
            StorageFolder folder = await p.PickSingleFolderAsync();
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            Windows.Storage.StorageFile sampleFile = await folder.CreateFileAsync("sample.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            Debug.Write("" + storageFolder.Path);
        }

        public Stream FileToStream(string fileName)
        {
            // 打开文件
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            // 把 byte[] 转换成 Stream
            Stream stream = new MemoryStream(bytes);
            return stream;


        }

        public void StreamToFile(Stream stream, string fileName)
        {
            // 把 Stream 转换成 byte[]
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            // 把 byte[] 写入文件
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(bytes);
        }


        private async Task<IReadOnlyList<StorageFile>> OpenDir() {

            FolderPicker p = new FolderPicker();
            p.FileTypeFilter.Add(".jpg");
            p.FileTypeFilter.Add(".png");
            StorageFolder folder = await p.PickSingleFolderAsync();
            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

            return fileList;
        }
        public async Task SaveImage(string imageName, string imageUri)
        {
            BackgroundDownloader backgroundDownload = new BackgroundDownloader();

            StorageFolder folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("ONE", CreationCollisionOption.OpenIfExists);
            StorageFile newFile = await folder.CreateFileAsync(imageName, CreationCollisionOption.OpenIfExists);


            Uri uri = new Uri(imageUri);
            DownloadOperation download = backgroundDownload.CreateDownload(uri, newFile);

            await download.StartAsync();

        }
        /// <summary>
        /// 点击“获取图片”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Get_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            //选取一个文件
            //var file = await picker.PickSingleFileAsync();
            //if (file != null)   //文件不为空则进行下一步
            //{
            //    _inputFile = file;
            //    await LoadFileAsync(file);
            //}
            IReadOnlyList<StorageFile> fileList = await OpenDir();
            foreach (StorageFile file in fileList)
            {
                if (file != null)   //文件不为空则进行下一步
                {
                    _inputFile = file;
                    await LoadFileAsync(file);
                }
            }
        }

        /// <summary>
        /// 从文件载入图片并显示
        /// </summary>
        /// <param name="file">图片</param>
        private async Task LoadFileAsync(StorageFile file)
        {
            try
            {
                // 显示图片
                BitmapImage src = new BitmapImage();
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await src.SetSourceAsync(stream);
                }
                MyImage.Source = src;

                LongSide.IsEnabled = true;
                SaveButton.IsEnabled = true;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
            }
        }

        /// <summary>
        /// 选择文件保存位置并进行下一步处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPEG image", new string[] { ".jpg" });
            picker.FileTypeChoices.Add("PNG image", new string[] { ".png" });
            picker.FileTypeChoices.Add("BMP image", new string[] { ".bmp" });
            picker.DefaultFileExtension = ".png";
            picker.SuggestedFileName = "Output file";
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await picker.PickSaveFileAsync();
            if (file != null && !String.IsNullOrEmpty(LongSide.Text))
            {
                uint longSide = uint.Parse(LongSide.Text);
                if (await LoadSaveFileAsync(_inputFile, file))
                    MsgBox.Text = "转换成功！" + file.Path;
                else
                    MsgBox.Text = "失败";
            }

        }

        /// <summary>
        /// 处理并保存图片
        /// </summary>
        /// <param name="inputFile">输入文件</param>
        /// <param name="outputFile">输出文件</param>
        /// <param name="longSide">长边长度</param>
        /// <returns>成功返回true，否则false。</returns>
        private async Task<bool> LoadSaveFileAsync(StorageFile inputFile, StorageFile outputFile)
        {
            try
            {
                Guid encoderId;
                switch (outputFile.FileType)
                {
                    case ".png":
                        encoderId = BitmapEncoder.PngEncoderId;
                        break;
                    case ".bmp":
                        encoderId = BitmapEncoder.BmpEncoderId;
                        break;
                    case ".jpg":
                    default:
                        encoderId = BitmapEncoder.JpegEncoderId;
                        break;
                }

                //图片处理部分
                using (IRandomAccessStream inputStream = await inputFile.OpenAsync(FileAccessMode.Read),
                           outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    //BitmapEncoder需要一个空的输出流; 但是用户可能已经选择了预先存在的文件，所以清零。
                    outputStream.Size = 0;

                    //从解码器获取像素数据。 我们对解码的像素应用用户请求的变换以利用解码器中的潜在优化。
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inputStream);
                    BitmapTransform transform = new BitmapTransform();

                    //原图尺寸比转换尺寸更小
                    
                    // 判断长边并按原图比例确定另一条边的长度
                    if (decoder.PixelHeight > decoder.PixelWidth)
                    {
                        uint longSide = uint.Parse((transform.ScaledHeight * 0.8).ToString());
                        transform.ScaledHeight = longSide;
                        transform.ScaledWidth = (uint)(decoder.PixelWidth * ((float)longSide / decoder.PixelHeight));
                    }
                    else
                    {
                        uint longSide = uint.Parse((transform.ScaledWidth * 0.8).ToString());
                        transform.ScaledWidth = longSide;
                        transform.ScaledHeight = (uint)(decoder.PixelHeight * ((float)longSide / decoder.PixelWidth));
                    }

                    // Fant是相对高质量的插值模式。
                    transform.InterpolationMode = BitmapInterpolationMode.Fant;

                    // BitmapDecoder指示最佳匹配本地存储的图像数据的像素格式和alpha模式。 这可以提供高性能的与或质量增益。
                    BitmapPixelFormat format = decoder.BitmapPixelFormat;
                    BitmapAlphaMode alpha = decoder.BitmapAlphaMode;

                    // PixelDataProvider提供对位图帧中像素数据的访问
                    PixelDataProvider pixelProvider = await decoder.GetPixelDataAsync(
                        format,
                        alpha,
                        transform,
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.ColorManageToSRgb
                        );

                    byte[] pixels = pixelProvider.DetachPixelData();

                    //将像素数据写入编码器。
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, outputStream);
                    //设置像素数据
                    encoder.SetPixelData(
                        format,
                        alpha,
                        transform.ScaledWidth,
                        transform.ScaledHeight,
                        decoder.DpiX,
                        decoder.DpiY,
                        pixels
                        );

                    await encoder.FlushAsync(); //异步提交和刷新所有图像数据（这一步保存图片到文件）
                    Debug.WriteLine("保存成功：" + outputFile.Path);
                    return true;
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return false;
            }
        }

    }
}
