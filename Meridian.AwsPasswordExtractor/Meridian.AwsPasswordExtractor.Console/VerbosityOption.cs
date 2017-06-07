// ----------------------------------------------------------------------------
// <copyright file="VerbosityOption.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Console
{
    /// <summary>
    /// Describes the various levels of logging verbosity available.
    /// Used by <see cref="Options" />. Pretty much mirrors NLog's logging
    /// levels. <see cref="System.Enum" /> created for easy parsing by
    /// <c>CommandLine</c>.
    /// </summary>
    public enum VerbosityOption
    {
        /// <summary>
        /// The verbosity option "Off".
        /// </summary>
        Off,

        /// <summary>
        /// The verbosity option "Debug".
        /// </summary>
        Debug,

        /// <summary>
        /// The verbosity option "Info".
        /// </summary>
        Info,

        /// <summary>
        /// The verbosity option "Warn".
        /// </summary>
        Warn,

        /// <summary>
        /// The verbosity option "Error".
        /// </summary>
        Error,

        /// <summary>
        /// The verbosity option "Fatal".
        /// </summary>
        Fatal
    }
}