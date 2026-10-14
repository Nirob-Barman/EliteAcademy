using EliteAcademy.Domain.Events;

namespace EliteAcademy.Domain.Entities
{
    public class PaymentGateway : BaseEntity
    {
        public string Slug { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Config { get; private set; } = string.Empty;   // encrypted JSON
        public bool IsActive { get; private set; } = true;
        public bool IsSandbox { get; private set; } = true;
        
        private readonly List<PaymentTransaction> _transactions = new();
        public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

        private PaymentGateway() { } // EF Core

        public static PaymentGateway Create(
            string slug,
            string name,
            string config,
            bool isActive = true,
            bool isSandbox = true)
        {
            Validate(slug, name, config);

            return new PaymentGateway
            {
                Slug = slug.Trim().ToLowerInvariant(),
                Name = name.Trim(),
                Config = config,
                IsActive = isActive,
                IsSandbox = isSandbox,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateDetails(
            string slug,
            string name,
            string encryptedConfig,
            bool isActive,
            bool isSandbox)
        {
            Validate(slug, name, encryptedConfig);

            Slug = slug.Trim().ToLowerInvariant();
            Name = name.Trim();
            Config = encryptedConfig;
            IsActive = isActive;
            IsSandbox = isSandbox;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void Validate(string slug, string name, string config)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Gateway slug is required.", nameof(slug));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Gateway name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(config))
                throw new ArgumentException("Gateway configuration is required.", nameof(config));
        }

        public void ToggleStatus()
        {
            IsActive = !IsActive;
        }

        public void MarkDeleted() => AddDomainEvent(new PaymentGatewayDeletedEvent(Name, Id));
    }
}
