

using System;

namespace VideoViewerDemo
{
    public class MyVideo
    {
        private string _name;
        private Uri _source;

        public MyVideo(string path)
        {
            Source = path;
            _source = new Uri(path);
        }

        public MyVideo(string path, string name)
        {
            _name = name;
            Source = path;
            _source = new Uri(path);
        }

        public string VideoTitle
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                }
            }
        }

        public string Source { get; }
    }
}