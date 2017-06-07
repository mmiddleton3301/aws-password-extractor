// ----------------------------------------------------------------------------
// <copyright file="FileSystemProvider.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using System;
    using System.IO;
    using Meridian.AwsPasswordExtractor.Logic.Definitions;

    /// <summary>
    /// Implements <see cref="IFileSystemProvider" />. 
    /// </summary>
    public class FileSystemProvider : IFileSystemProvider
    {
        /// <summary>
        /// Implements
        /// <see cref="IFileSystemProvider.ReadFileInfoAsString(FileInfo)" />. 
        /// </summary>
        /// <param name="fileInfo">
        /// An instance of <see cref="FileInfo" /> describing where the file
        /// to be read.
        /// </param>
        /// <returns>
        /// The contents of the file as a <see cref="string" /> value. 
        /// </returns>
        public string ReadFileInfoAsString(FileInfo fileInfo)
        {
            string toReturn = null;

            using (FileStream fileStream = fileInfo.Open(FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    toReturn = streamReader.ReadToEnd();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Implements
        /// <see cref="IFileSystemProvider.WriteStringToFileInfo(FileInfo, string)" />. 
        /// </summary>
        /// <param name="fileInfo">
        /// An instance of <see cref="FileInfo" /> describing where to write
        /// the <see cref="string" /> value to. 
        /// </param>
        /// <param name="toWrite">
        /// A <see cref="string" /> value. 
        /// </param>
        public void WriteStringToFileInfo(FileInfo fileInfo, string toWrite)
        {
            using (FileStream fileStream = fileInfo.Open(FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(toWrite);
                }
            }
        }
    }
}