using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Internet_shaop_do_refactora1
{
    internal class step1
    {
        string _connectionString;

        public class UserWithOrdersDto
        {
            public int UserId { get; set; }
            public string Name { get; set; }
            public List<OrderDto> Orders { get; set; }
        }

        public class OrderDto
        {
            public int OrderId { get; set; }
            public decimal Total { get; set; } // Исправлено: Total должен быть decimal, а не int
        }

        public List<UserWithOrdersDto> GetUserWithOrders()
        {
            var result = new List<UserWithOrdersDto>();

            // 1 using для автоматического закрытия
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // 2 защита от sql инъекций (параметризированный запрос)
                const string usersSql = "SELECT Id, Name FROM Users";

                using (var userCmd = new SqlCommand(usersSql, connection))
                using (var usersReader = userCmd.ExecuteReader())
                {
                    while (usersReader.Read())
                    {
                        var userId = usersReader.GetInt32(usersReader.GetOrdinal("Id"));
                        var userName = usersReader.GetString(usersReader.GetOrdinal("Name"));

                        // Все еще N+1 проблема, но ресурсы теперь закрываются
                        const string orderSql = "SELECT Id, Total FROM Orders WHERE UserID=@UserId";
                        using (var ordersCmd = new SqlCommand(orderSql, connection))
                        {
                            ordersCmd.Parameters.AddWithValue("@userId", userId);
                            using (var ordersReader = ordersCmd.ExecuteReader())
                            {
                                var orders = new List<OrderDto>();
                                while (ordersReader.Read())
                                {
                                    orders.Add(new OrderDto
                                    {
                                        OrderId = ordersReader.GetInt32(ordersReader.GetOrdinal("Id")),
                                        Total = ordersReader.GetDecimal(ordersReader.GetOrdinal("Total"))
                                    });
                                }

                                result.Add(new UserWithOrdersDto
                                {
                                    UserId = userId,
                                    Name = userName,
                                    Orders = orders
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}