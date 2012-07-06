using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Markup;


namespace おっぱいマスター
{
   
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.FolderBrowserDialog folderDialog;
        private System.Windows.Forms.SaveFileDialog saveDialog;
        private string imagesDirectory;
        private ObservableCollection<ObjViewModel> objItems;
        private bool isDownFirst;
        private bool isMouseDown;
        private Point mouseDownPoint;
        private List<Polygon> nowRects;
        private int beforeSelectedIndex;
        private Color rectColor;

        public MainWindow()
        {
            InitializeComponent();
            ControlInitialize();
            imagesDirectory = Directory.GetCurrentDirectory();
            objItems = new ObservableCollection<ObjViewModel>();
            listBox_files.ItemsSource = objItems;
            isDownFirst = true;
            isMouseDown = false;
            mouseDownPoint = new Point();
            nowRects = new List<Polygon>();
            beforeSelectedIndex = -1;
            rectColor = Colors.Red;
        }

        #region PrivateFunction

        //ダイアログ初期化
        private void ControlInitialize()
        {
            this.folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.folderDialog.Description = "画像フォルダ選択";
            this.folderDialog.ShowNewFolderButton = false;

            this.saveDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveDialog.RestoreDirectory = true;
            this.saveDialog.OverwritePrompt = true;
            this.saveDialog.Filter ="テキストファイル(*.txt)|*.txt";
            this.saveDialog.Title = "ファイルにエキスポート";
        }

        //画像を開く
        private void OpenDirectory()
        {
            System.Windows.Forms.DialogResult result = this.folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                imagesDirectory = this.folderDialog.SelectedPath;
                ChangeDirectory(imagesDirectory);
            }
        }

        //読み込む画像のディレクトリを変える
        private void ChangeDirectory(string directoryPath)
        {
            string[] files = Directory.GetFiles(directoryPath,"*",SearchOption.AllDirectories);
            objItems.Clear();
            foreach (string s in files)
            {
                //この拡張子意外は追加しない
                if (s.Contains(".jpg") || s.Contains(".png") || s.Contains(".gif") || s.Contains(".bmp"))
                {
                    objItems.Add(new ObjViewModel(s));
                }
            }
        }

        //画像を切り替える
        private void ChangeImage(string filePath,int index)
        {
            
            BitmapImage img = new BitmapImage(new Uri(filePath,UriKind.Absolute));
            this.image_preview.Source = img;
            if (objItems[index].ObjRects != null)
            {
                LoadRect(index);
            }
            else
            {
                NewRect();
            }
            

        }

        //次の画像へ
        private void NextImage()
        {
            SaveRect(listBox_files.SelectedIndex);
            if (listBox_files.SelectedIndex < listBox_files.Items.Count)
            {
                listBox_files.SelectedIndex += 1;
            
            }
        }

        //前の画像へ
        private void PrevImage()
        {
            SaveRect(listBox_files.SelectedIndex);
            if (listBox_files.SelectedIndex > 0)
            {
                listBox_files.SelectedIndex -= 1;
              
            }
        }

        //画像の四角形情報を保存する
        private void SaveRect(int index)
        {
            try
            {
                List<Rect> rect_list = new List<Rect>();
                foreach (Polygon p in nowRects)
                {
                    rect_list.Add(new Rect((int)p.Points[0].X, (int)p.Points[0].Y, (int)p.Points[2].X - (int)p.Points[0].X, (int)p.Points[2].Y - (int)p.Points[0].Y));
                }
                objItems[index].ObjRects = rect_list;
            }
            catch (Exception) 
            {
                MessageBox.Show("四角形を右から左に描いた可能性があります。マウスは必ず左上から右下へドラッグし、四角形を描画してください。");
                NewRect();
            }
        }

        //画像の四角形情報を新規作成する
        private void NewRect()
        {
            foreach (Polygon p in nowRects)
            {
                grid_image.Children.Remove(p);
            }
            nowRects.Clear();

        }

        //画像の四角形情報を読み込む
        private void LoadRect(int index)
        {
            NewRect();
            foreach (Rect r in objItems[index].ObjRects)
            {
                nowRects.Add(new Polygon());
                nowRects[nowRects.Count - 1].Points.Add(new Point(r.X,r.Y));
                nowRects[nowRects.Count - 1].Points.Add(new Point(r.Right, r.Y));
                nowRects[nowRects.Count - 1].Points.Add(new Point(r.Right, r.Bottom));
                nowRects[nowRects.Count - 1].Points.Add(new Point(r.X, r.Bottom));
                nowRects[nowRects.Count - 1].Stroke = new SolidColorBrush(rectColor);
                nowRects[nowRects.Count - 1].StrokeThickness = 5;
                grid_image.Children.Add(nowRects[nowRects.Count - 1]);
            }
        }

        //エクスポート
        private void Export()
        {
            SaveRect(listBox_files.SelectedIndex);
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (saveDialog.FileName == "")
            {
                return;
            }

            StreamWriter write = new StreamWriter(new FileStream(saveDialog.FileName, FileMode.Create));

            foreach (ObjViewModel item in objItems)
            {
                if (item.ObjRects!=null)
                {
                    if (item.ObjRects.Count != 0)
                    {
                        /*----------------ここで出力フォーマットを決めている-------------------------------*/
                        string str = "";
                        Uri u1 = new Uri(imagesDirectory);
                        Uri u2 = new Uri(item.FilePath);
                        str += u1.MakeRelativeUri(u2).ToString() + " " + item.ObjRects.Count.ToString();
                        foreach (Rect r in item.ObjRects)
                        {
                            str += " " + r.X.ToString() + " " + r.Y.ToString() + " " + r.Width.ToString() + " " + r.Height.ToString();
                        }
                        write.WriteLine(str);
                    }
                }
            }
            write.Close();
        }

        #endregion


       

        #region UIEvent

        //開くをクリック
        private void button_open_Click(object sender, RoutedEventArgs e)
        {
            OpenDirectory();
        }

        //リストボックスのセレクトチェンジ
        private void listBox_files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (beforeSelectedIndex != -1)
            {
                SaveRect(beforeSelectedIndex);
            }
                ChangeImage(objItems[listBox_files.SelectedIndex].FilePath, listBox_files.SelectedIndex);
                beforeSelectedIndex = listBox_files.SelectedIndex;
           
        }


        //前へをクリック
        private void button_prev_Click(object sender, RoutedEventArgs e)
        {
            PrevImage();
        }

        //次へをクリック
        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            NextImage();
        }

        


        //画像上でマウスダウン
        private void grid_image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isDownFirst == true)
            {
                mouseDownPoint = e.GetPosition(image_preview);
                isDownFirst = false;
                nowRects.Add(new Polygon());
                nowRects[nowRects.Count - 1].Points.Add(mouseDownPoint);
                nowRects[nowRects.Count - 1].Points.Add(mouseDownPoint);
                nowRects[nowRects.Count - 1].Points.Add(mouseDownPoint);
                nowRects[nowRects.Count - 1].Points.Add(mouseDownPoint);
                nowRects[nowRects.Count - 1].Stroke = new SolidColorBrush(rectColor);
                nowRects[nowRects.Count - 1].StrokeThickness = 5;
                grid_image.Children.Add(nowRects[nowRects.Count - 1]);

            }
            isMouseDown = true;
        }

        //画像上でマウス移動
        private void grid_image_MouseMove(object sender, MouseEventArgs e)
        {
            if (nowRects.Count != 0 && isMouseDown == true)
            {
                nowRects[nowRects.Count - 1].Points[1] = new Point(e.GetPosition(image_preview).X, mouseDownPoint.Y);
                nowRects[nowRects.Count - 1].Points[2] = new Point(e.GetPosition(image_preview).X, e.GetPosition(image_preview).Y);
                nowRects[nowRects.Count - 1].Points[3] = new Point(mouseDownPoint.X, e.GetPosition(image_preview).Y);
            }
        }

        //画像上でマウスアップ
        private void grid_image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDownFirst = true;
            isMouseDown = false;
            
        }

        //やり直しをクリック
        private void button_undo_Click(object sender, RoutedEventArgs e)
        {
            NewRect();
        }

        //保存をクリック
        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            SaveRect(listBox_files.SelectedIndex);
        }

        
        //エクスポートをクリック
        private void button_export_Click(object sender, RoutedEventArgs e)
        {
            Export();
        }


        //ショートカットキー検地
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.X:
                    NextImage();
                    break;
                case Key.Z:
                    PrevImage();
                    break;
                case Key.S:
                    SaveRect(listBox_files.SelectedIndex);
                    break;
                case Key.N:
                    NewRect();
                    break;
                case Key.O:
                    OpenDirectory();
                    break;
                case Key.E:
                    Export();
                    break;
            }
        }

        #endregion

        




    }
}
