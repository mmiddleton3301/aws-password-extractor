// ----------------------------------------------------------------------------
// <copyright file="InstanceDetail.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Models
{
    using System.Net;

    /// <summary>
    /// Contains information regarding a particular instance, in particular
    /// its <see cref="InstanceDetail.IPAddress" /> and
    /// <see cref="InstanceDetail.Password" />.  
    /// </summary>
    public class InstanceDetail
    {
        /// <summary>
        /// Gets or sets the IP address of the instance.
        /// </summary>
        public IPAddress IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password for the root/administrator user of
        /// the instance.
        /// </summary>
        public string Password
        {
            get;
            set;
        }
    }
}