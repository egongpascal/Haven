using Haven.Domain.DTO;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public interface INotificationService
    {
        Task SendEmergencyNotificationAsync(EmergencyRequest request);
        Task SendEmergencyResolvedNotificationAsync(string emergencyId, ResolveEmergencyRequest request);
    }

    public class NotificationService : INotificationService
    {
        public async Task SendEmergencyNotificationAsync(EmergencyRequest request)
        {
            // TODO: Integrate with SMS/email providers (e.g., Twilio, SendGrid)
            // Example: Send SMS to emergency contacts
            // Example: Send email to group admin
        }

        public async Task SendEmergencyResolvedNotificationAsync(string emergencyId, ResolveEmergencyRequest request)
        {
            // TODO: Integrate with SMS/email providers for resolution notification
        }
    }
}
