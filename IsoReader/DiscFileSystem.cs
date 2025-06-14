//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using DiscUtils.Streams;

namespace DiscUtils
{
    /// <summary>
    /// Provides the base class for all file systems.
    /// </summary>
    public abstract class DiscFileSystem :
#if !NETSTANDARD
        MarshalByRefObject, 
#endif
        IFileSystem, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the DiscFileSystem class.
        /// </summary>
        protected DiscFileSystem()
        {
            Options = new DiscFileSystemOptions();
        }

        /// <summary>
        /// Initializes a new instance of the DiscFileSystem class.
        /// </summary>
        /// <param name="defaultOptions">The options instance to use for this file system instance.</param>
        protected DiscFileSystem(DiscFileSystemOptions defaultOptions)
        {
            Options = defaultOptions;
        }

        /// <summary>
        /// Finalizes an instance of the DiscFileSystem class.
        /// </summary>
        ~DiscFileSystem()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the file system options, which can be modified.
        /// </summary>
        public virtual DiscFileSystemOptions Options { get; }

        /// <summary>
        /// Gets a friendly description of the file system type.
        /// </summary>
        public abstract string FriendlyName { get; }

        /// <summary>
        /// Gets a value indicating whether the file system is read-only or read-write.
        /// </summary>
        /// <returns>true if the file system is read-write.</returns>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// Gets the root directory of the file system.
        /// </summary>
        public virtual DiscDirectoryInfo Root
        {
            get { return new DiscDirectoryInfo(this, string.Empty); }
        }

        /// <summary>
        /// Gets the volume label.
        /// </summary>
        public virtual string VolumeLabel
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets a value indicating whether the file system is thread-safe.
        /// </summary>
        public virtual bool IsThreadSafe
        {
            get { return false; }
        }

        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        public virtual void CopyFile(string sourceFile, string destinationFile)
        {
            CopyFile(sourceFile, destinationFile, false);
        }

        /// <summary>
        /// Copies an existing file to a new file, allowing overwriting of an existing file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        /// <param name="overwrite">Whether to permit over-writing of an existing file.</param>
        public abstract void CopyFile(string sourceFile, string destinationFile, bool overwrite);

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="path">The path of the new directory.</param>
        public abstract void CreateDirectory(string path);

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        public abstract void DeleteDirectory(string path);

        /// <summary>
        /// Deletes a directory, optionally with all descendants.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        /// <param name="recursive">Determines if the all descendants should be deleted.</param>
        public virtual void DeleteDirectory(string path, bool recursive)
        {
            if (recursive)
            {
                foreach (string dir in GetDirectories(path))
                {
                    DeleteDirectory(dir, true);
                }

                foreach (string file in GetFiles(path))
                {
                    DeleteFile(file);
                }
            }

            DeleteDirectory(path);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        public abstract void DeleteFile(string path);

        /// <summary>
        /// Indicates if a directory exists.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>true if the directory exists.</returns>
        public abstract bool DirectoryExists(string path);

        /// <summary>
        /// Indicates if a file exists.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>true if the file exists.</returns>
        public abstract bool FileExists(string path);

        /// <summary>
        /// Indicates if a file or directory exists.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>true if the file or directory exists.</returns>
        public virtual bool Exists(string path)
        {
            return FileExists(path) || DirectoryExists(path);
        }

        /// <summary>
        /// Gets the names of subdirectories in a specified directory.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>Array of directories.</returns>
        public virtual string[] GetDirectories(string path)
        {
            return GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Gets the names of subdirectories in a specified directory matching a specified
        /// search pattern.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <param name="searchPattern">The search string to match against.</param>
        /// <returns>Array of directories matching the search pattern.</returns>
        public virtual string[] GetDirectories(string path, string searchPattern)
        {
            return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Gets the names of subdirectories in a specified directory matching a specified
        /// search pattern, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <param name="searchPattern">The search string to match against.</param>
        /// <param name="searchOption">Indicates whether to search subdirectories.</param>
        /// <returns>Array of directories matching the search pattern.</returns>
        public abstract string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Gets the names of files in a specified directory.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>Array of files.</returns>
        public virtual string[] GetFiles(string path)
        {
            return GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Gets the names of files in a specified directory.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <param name="searchPattern">The search string to match against.</param>
        /// <returns>Array of files matching the search pattern.</returns>
        public virtual string[] GetFiles(string path, string searchPattern)
        {
            return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Gets the names of files in a specified directory matching a specified
        /// search pattern, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <param name="searchPattern">The search string to match against.</param>
        /// <param name="searchOption">Indicates whether to search subdirectories.</param>
        /// <returns>Array of files matching the search pattern.</returns>
        public abstract string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Gets the names of all files and subdirectories in a specified directory.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <returns>Array of files and subdirectories matching the search pattern.</returns>
        public abstract string[] GetFileSystemEntries(string path);

        /// <summary>
        /// Gets the names of files and subdirectories in a specified directory matching a specified
        /// search pattern.
        /// </summary>
        /// <param name="path">The path to search.</param>
        /// <param name="searchPattern">The search string to match against.</param>
        /// <returns>Array of files and subdirectories matching the search pattern.</returns>
        public abstract string[] GetFileSystemEntries(string path, string searchPattern);

        /// <summary>
        /// Moves a directory.
        /// </summary>
        /// <param name="sourceDirectoryName">The directory to move.</param>
        /// <param name="destinationDirectoryName">The target directory name.</param>
        public abstract void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName);

        /// <summary>
        /// Moves a file.
        /// </summary>
        /// <param name="sourceName">The file to move.</param>
        /// <param name="destinationName">The target file name.</param>
        public virtual void MoveFile(string sourceName, string destinationName)
        {
            MoveFile(sourceName, destinationName, false);
        }

        /// <summary>
        /// Moves a file, allowing an existing file to be overwritten.
        /// </summary>
        /// <param name="sourceName">The file to move.</param>
        /// <param name="destinationName">The target file name.</param>
        /// <param name="overwrite">Whether to permit a destination file to be overwritten.</param>
        public abstract void MoveFile(string sourceName, string destinationName, bool overwrite);

        /// <summary>
        /// Opens the specified file.
        /// </summary>
        /// <param name="path">The full path of the file to open.</param>
        /// <param name="mode">The file mode for the created stream.</param>
        /// <returns>The new stream.</returns>
        public virtual SparseStream OpenFile(string path, FileMode mode)
        {
            return OpenFile(path, mode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Opens the specified file.
        /// </summary>
        /// <param name="path">The full path of the file to open.</param>
        /// <param name="mode">The file mode for the created stream.</param>
        /// <param name="access">The access permissions for the created stream.</param>
        /// <returns>The new stream.</returns>
        public abstract SparseStream OpenFile(string path, FileMode mode, FileAccess access);

        /// <summary>
        /// Gets the attributes of a file or directory.
        /// </summary>
        /// <param name="path">The file or directory to inspect.</param>
        /// <returns>The attributes of the file or directory.</returns>
        public abstract FileAttributes GetAttributes(string path);

        /// <summary>
        /// Sets the attributes of a file or directory.
        /// </summary>
        /// <param name="path">The file or directory to change.</param>
        /// <param name="newValue">The new attributes of the file or directory.</param>
        public abstract void SetAttributes(string path, FileAttributes newValue);

        /// <summary>
        /// Gets the creation time (in local time) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>The creation time.</returns>
        public virtual DateTime GetCreationTime(string path)
        {
            return GetCreationTimeUtc(path).ToLocalTime();
        }

        /// <summary>
        /// Sets the creation time (in local time) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="newTime">The new time to set.</param>
        public virtual void SetCreationTime(string path, DateTime newTime)
        {
            SetCreationTimeUtc(path, newTime.ToUniversalTime());
        }

        /// <summary>
        /// Gets the creation time (in UTC) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>The creation time.</returns>
        public abstract DateTime GetCreationTimeUtc(string path);

        /// <summary>
        /// Sets the creation time (in UTC) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="newTime">The new time to set.</param>
        public abstract void SetCreationTimeUtc(string path, DateTime newTime);

        /// <summary>
        /// Gets the last access time (in local time) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>The last access time.</returns>
        public virtual DateTime GetLastAccessTime(string path)
        {
            return GetLastAccessTimeUtc(path).ToLocalTime();
        }

        /// <summary>
        /// Sets the last access time (in local time) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="newTime">The new time to set.</param>
        public virtual void SetLastAccessTime(string path, DateTime newTime)
        {
            SetLastAccessTimeUtc(path, newTime.ToUniversalTime());
        }

        /// <summary>
        /// Gets the last access time (in UTC) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>The last access time.</returns>
        public abstract DateTime GetLastAccessTimeUtc(string path);

        /// <summary>
        /// Sets the last access time (in UTC) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="newTime">The new time to set.</param>
        public abstract void SetLastAccessTimeUtc(string path, DateTime newTime);

        /// <summary>
        /// Gets the last modification time (in local time) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>The last write time.</returns>
        public virtual DateTime GetLastWriteTime(string path)
        {
            return GetLastWriteTimeUtc(path).ToLocalTime();
        }

        /// <summary>
        /// Sets the last modification time (in local time) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="newTime">The new time to set.</param>
        public virtual void SetLastWriteTime(string path, DateTime newTime)
        {
            SetLastWriteTimeUtc(path, newTime.ToUniversalTime());
        }

        /// <summary>
        /// Gets the last modification time (in UTC) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>The last write time.</returns>
        public abstract DateTime GetLastWriteTimeUtc(string path);

        /// <summary>
        /// Sets the last modification time (in UTC) of a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="newTime">The new time to set.</param>
        public abstract void SetLastWriteTimeUtc(string path, DateTime newTime);

        /// <summary>
        /// Gets the length of a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The length in bytes.</returns>
        public abstract long GetFileLength(string path);

        /// <summary>
        /// Gets an object representing a possible file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The representing object.</returns>
        /// <remarks>The file does not need to exist.</remarks>
        public virtual DiscFileInfo GetFileInfo(string path)
        {
            return new DiscFileInfo(this, path);
        }

        /// <summary>
        /// Gets an object representing a possible directory.
        /// </summary>
        /// <param name="path">The directory path.</param>
        /// <returns>The representing object.</returns>
        /// <remarks>The directory does not need to exist.</remarks>
        public virtual DiscDirectoryInfo GetDirectoryInfo(string path)
        {
            return new DiscDirectoryInfo(this, path);
        }

        /// <summary>
        /// Gets an object representing a possible file system object (file or directory).
        /// </summary>
        /// <param name="path">The file system path.</param>
        /// <returns>The representing object.</returns>
        /// <remarks>The file system object does not need to exist.</remarks>
        public virtual DiscFileSystemInfo GetFileSystemInfo(string path)
        {
            return new DiscFileSystemInfo(this, path);
        }

        /// <summary>
        /// Reads the boot code of the file system into a byte array.
        /// </summary>
        /// <returns>The boot code, or <c>null</c> if not available.</returns>
        public virtual byte[] ReadBootCode()
        {
            return null;
        }

        /// <summary>
        /// Size of the Filesystem in bytes
        /// </summary>
        public abstract long Size { get; }
 
        /// <summary>
        /// Used space of the Filesystem in bytes
        /// </summary>
        public abstract long UsedSpace { get; }
 
        /// <summary>
        /// Available space of the Filesystem in bytes
        /// </summary>
        public abstract long AvailableSpace { get; }

        #region IDisposable Members

        /// <summary>
        /// Disposes of this instance, releasing all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        /// <param name="disposing">The value <c>true</c> if Disposing.</param>
        protected virtual void Dispose(bool disposing) {}

        #endregion
    }
}