using System;
using System.IO;

namespace BCEngine.IO
{
    // Implementing IDisposable is crucial when handling open file streams
    public class FileHandler : IDisposable
    {
        private string _fileName;
        private StreamReader _reader;

        public FileHandler(string fileName)
        {
            this._fileName = fileName;
        }

        public void CreateFile(string content)
        {
            // Ensure directory exists before creating the file
            CreateDirectory();

            if (!File.Exists(_fileName))
            {
                File.WriteAllText(_fileName, content);
            }
        }

        public void CreateDirectory()
        {
            string directory = Path.GetDirectoryName(_fileName);

            // Only create directory if a directory path actually exists in the string
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // Modified to return the string read, or null if end of file
        public string ReadLine()
        {
            // Initialize the reader only when we actually start reading
            if (_reader == null)
            {
                if (File.Exists(_fileName))
                {
                    _reader = new StreamReader(_fileName);
                }
                else
                {
                    return null; // File doesn't exist
                }
            }

            // Read the next line
            return _reader.ReadLine();
        }

        // Clean up the StreamReader when we are done
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }
        }
    }
}