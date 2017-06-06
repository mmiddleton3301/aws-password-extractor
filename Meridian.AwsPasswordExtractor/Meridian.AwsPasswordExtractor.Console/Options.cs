// ----------------------------------------------------------------------------
// <copyright file="Options.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Console
{
    using CommandLine;

    /// <summary>
    /// An options class, used by the <see cref="CommandLine.Parser" /> to
    /// provide option metadata and clean parsing.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets a role ARN to assume. Optional.
        /// </summary>
        [Option(HelpText =
            "If assuming a particular role as part of your request, " +
            "specify the role ARN with this option.")]
        public string RoleArn
        {
            get;
            set;
        }
    }
}
