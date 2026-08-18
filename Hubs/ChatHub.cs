using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace BookStore.Hubs
{
    public class ChatMessageItem
    {
        public string Sender { get; set; } = string.Empty; // "Customer" or "Staff"
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ChatHub : Hub
    {
        // Thread-safe in-memory stores matching ChatServer.java
        private static readonly ConcurrentDictionary<string, List<ChatMessageItem>> ChatHistories = new();
        private static readonly ConcurrentDictionary<string, string> CustomerConnectionMap = new(); // CustomerName -> ConnectionId

        public const string StaffGroupName = "StaffGroup";

        public async Task RegisterStaff()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, StaffGroupName);

            // Send all customer histories to this staff member
            foreach (var kvp in ChatHistories)
            {
                await Clients.Caller.SendAsync("LoadCustomerHistory", kvp.Key, kvp.Value);
            }
        }

        public async Task RegisterCustomer(string customerName)
        {
            CustomerConnectionMap[customerName] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerName}");

            if (ChatHistories.TryGetValue(customerName, out var history))
            {
                await Clients.Caller.SendAsync("LoadHistory", history);
            }
        }

        public async Task SendMessageFromCustomer(string customerName, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            var msgItem = new ChatMessageItem
            {
                Sender = "Customer",
                SenderName = customerName,
                Content = message.Trim(),
                Timestamp = DateTime.UtcNow
            };

            ChatHistories.AddOrUpdate(
                customerName,
                new List<ChatMessageItem> { msgItem },
                (_, list) =>
                {
                    list.Add(msgItem);
                    return list;
                }
            );

            // Notify all Staff members
            await Clients.Group(StaffGroupName).SendAsync("ReceiveCustomerMessage", customerName, msgItem);

            // Confirm back to caller
            await Clients.Caller.SendAsync("MessageSent", msgItem);
        }

        public async Task SendMessageFromStaff(string targetCustomer, string message, string staffName)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(targetCustomer)) return;

            var msgItem = new ChatMessageItem
            {
                Sender = "Staff",
                SenderName = string.IsNullOrWhiteSpace(staffName) ? "Tư vấn viên MindBook" : staffName,
                Content = message.Trim(),
                Timestamp = DateTime.UtcNow
            };

            ChatHistories.AddOrUpdate(
                targetCustomer,
                new List<ChatMessageItem> { msgItem },
                (_, list) =>
                {
                    list.Add(msgItem);
                    return list;
                }
            );

            // Send to the target customer
            await Clients.Group($"Customer_{targetCustomer}").SendAsync("ReceiveStaffMessage", msgItem);

            // Sync with all staff members
            await Clients.Group(StaffGroupName).SendAsync("SyncStaffMessage", targetCustomer, msgItem);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove disconnected customer mapping
            var item = CustomerConnectionMap.FirstOrDefault(kvp => kvp.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(item.Key))
            {
                CustomerConnectionMap.TryRemove(item.Key, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
