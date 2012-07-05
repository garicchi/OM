using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;

namespace おっぱいマスター
{
    public class ObjViewModel:INotifyPropertyChanged
    {
        private string filePath;
        private string fileName;
        private List<Rect> objRects;
        public ObjViewModel(string filePath)
        {
            this.filePath = filePath;
            this.fileName = System.IO.Path.GetFileName(filePath);
            objRects = null;
        }

        #region INotifyPropertyChangedメンバ
        public event PropertyChangedEventHandler PropertyChanged;
        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public string FilePath
        {
            get { return this.filePath; }
            set 
            { 
                this.filePath = value;
                FirePropertyChanged("FilePath");
            }
        }

        public string FileName
        {
            get { return this.fileName; }
            set
            {
                this.fileName = value;
                FirePropertyChanged("FileName");
            }
        }

        public SolidColorBrush IsFinished
        {
            get 
            {
                if (objRects != null&&objRects.Count!=0)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
            
        }

        public List<Rect> ObjRects
        {
            get { return this.objRects; }
            set 
            { 
                this.objRects = value;
                FirePropertyChanged("ObjRects");
                FirePropertyChanged("IsFinished");
            }
        }

       
    }
}
