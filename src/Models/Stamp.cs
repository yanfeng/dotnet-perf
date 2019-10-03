using System;
using System.Collections.Generic;

namespace DotNet.Perf.Models
{
    [Serializable]
    public class Stamp<TId> : IEquatable<Stamp<TId>>
    {
        public Stamp(TId createdBy, TId createdOnBehalfOf, DateTimeOffset createdTimestamp, 
            TId lastModifiedBy, TId lastModifiedOnBehalfOf, DateTimeOffset lastModifiedTimestamp)
        {
            this.createdBy = createdBy;
            this.createdOnBehalfOf = createdOnBehalfOf;
            this.createdTimestamp = createdTimestamp;
            this.lastModifiedBy = lastModifiedBy;
            this.lastModifiedOnBehalfOf = lastModifiedOnBehalfOf;
            this.lastModifiedTimestamp = lastModifiedTimestamp;
        }

        public TId CreatedBy
        {
            get { return createdBy; }
        }

        public TId CreatedOnBehalfOf
        {
            get { return createdOnBehalfOf; }
        }

        public DateTimeOffset CreatedTimestamp
        {
            get { return createdTimestamp; }
        }

        public TId LastModifiedBy
        {
            get { return lastModifiedBy; }
        }

        public TId LastModifiedOnBehalfOf
        {
            get { return lastModifiedOnBehalfOf; }
        }

        public DateTimeOffset LastModifiedTimestamp
        {
            get { return lastModifiedTimestamp; }
        }

        public bool Equals(Stamp<TId> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TId>.Default.Equals(createdBy, other.createdBy) &&
                   EqualityComparer<TId>.Default.Equals(createdOnBehalfOf, other.createdOnBehalfOf) &&
                   createdTimestamp.Equals(other.createdTimestamp) &&
                   EqualityComparer<TId>.Default.Equals(lastModifiedBy, other.lastModifiedBy) &&
                   EqualityComparer<TId>.Default.Equals(lastModifiedOnBehalfOf, other.lastModifiedOnBehalfOf) &&
                   lastModifiedTimestamp.Equals(other.lastModifiedTimestamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Stamp<TId>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = EqualityComparer<TId>.Default.GetHashCode(createdBy);
                hashCode = (hashCode*397) ^ EqualityComparer<TId>.Default.GetHashCode(createdOnBehalfOf);
                hashCode = (hashCode*397) ^ createdTimestamp.GetHashCode();
                hashCode = (hashCode*397) ^ EqualityComparer<TId>.Default.GetHashCode(lastModifiedBy);
                hashCode = (hashCode*397) ^ EqualityComparer<TId>.Default.GetHashCode(lastModifiedOnBehalfOf);
                hashCode = (hashCode*397) ^ lastModifiedTimestamp.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Stamp<TId> left, Stamp<TId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Stamp<TId> left, Stamp<TId> right)
        {
            return !Equals(left, right);
        }

        #region Fields

        private readonly TId createdBy;
        private readonly TId createdOnBehalfOf;
        private readonly DateTimeOffset createdTimestamp;
        private readonly TId lastModifiedBy;
        private readonly TId lastModifiedOnBehalfOf;
        private readonly DateTimeOffset lastModifiedTimestamp;

        #endregion
    }

    [Serializable]
    public class Stamp : Stamp<AuditUser>
    {
        public Stamp(AuditUser createdBy, AuditUser createdOnBehalfOf, DateTimeOffset createdTimestamp,
                     AuditUser lastModifiedBy, AuditUser lastModifiedOnBehalfOf, DateTimeOffset lastModifiedTimestamp)
            : base(createdBy, createdOnBehalfOf, createdTimestamp, lastModifiedBy, lastModifiedOnBehalfOf, lastModifiedTimestamp)
        {
        }
    }
}