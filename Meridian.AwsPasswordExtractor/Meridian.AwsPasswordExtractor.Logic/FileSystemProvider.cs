// ----------------------------------------------------------------------------
// <copyright file="FileSystemProvider.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
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
    }
}