// ----------------------------------------------------------------------------
// <copyright file="IFileSystemProvider.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Definitions
{
    using System.IO;

    /// <summary>
    /// Describes the operations of the file system provider.
    /// </summary>
    public interface IFileSystemProvider
    {
        /// <summary>
        /// Reads a specified <paramref name="fileInfo" />'s content and
        /// returns it as a <see cref="string" /> value. 
        /// </summary>
        /// <param name="fileInfo">
        /// An instance of <see cref="FileInfo" /> describing where the file
        /// to be read.
        /// </param>
        /// <returns>
        /// The contents of the file as a <see cref="string" /> value. 
        /// </returns>
        string ReadFileInfoAsString(FileInfo fileInfo);

        /// <summary>
        /// Writes a specified <see cref="string" /> value to a particular
        /// <paramref name="fileInfo" />.
        /// If the file exists already, then it will be overwritten.
        /// </summary>
        /// <param name="fileInfo">
        /// An instance of <see cref="FileInfo" /> describing where to write
        /// the <see cref="string" /> value to. 
        /// </param>
        /// <param name="toWrite">
        /// A <see cref="string" /> value. 
        /// </param>
        void WriteStringToFileInfo(FileInfo fileInfo, string toWrite);
    }
}