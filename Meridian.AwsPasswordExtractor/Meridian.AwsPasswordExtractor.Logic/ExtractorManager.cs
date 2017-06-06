// ----------------------------------------------------------------------------
// <copyright file="ExtractorManager.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Implements <see cref="IExtractorManager" />. 
    /// </summary>
    public class ExtractorManager : IExtractorManager
    {
        /// <summary>
        /// Implements <see cref="IExtractorManager.ExtractDetails()" />. 
        /// </summary>
        /// <returns>
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </returns>
        public InstanceDetail[] ExtractDetails()
        {
            InstanceDetail[] toReturn = null;

            // TODO: This needs implementing!
            return toReturn;
        }
    }
}
