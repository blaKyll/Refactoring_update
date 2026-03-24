using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Internet_shaop_do_refactora1
{
    public class OrderDto
    {
        //для примера
        public int OrderId { get; set; }
        public decimal Total { get; set; }
    }

    public class UserWithOrdersDto
    {
        // реализация класса
        public int UserId { get; set; }  // Исправлено: Id -> UserId для консистентности
        public string Name { get; set; }
        public List<OrderDto> Orders { get; set; }
    }

    internal class step2
    {
        string _connectionString; // Для примера!

        public List<UserWithOrdersDto> GetUsersWithOrdersOptimized()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                //1 Один запрос, получаем всех пользователей и их заказы через Join
                const string sql = @"SELECT
                u.Id AS UserId,
                u.Name AS Username,
                o.Id AS OrderId,
                o.Total AS OrderTotal
                FROM Users u LEFT JOIN Orders o ON u.Id = o.UserId ORDER BY u.Id, o.Id";

                var usersMap = new Dictionary<int, UserWithOrdersDto>();

                using (var cmd = new SqlCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var userId = reader.GetInt32(reader.GetOrdinal("UserId"));
                        var userName = reader.GetString(reader.GetOrdinal("Username"));

                        // Получаем или создаем пользователя в словаре
                        if (!usersMap.TryGetValue(userId, out var user))
                        {
                            user = new UserWithOrdersDto()
                            {
                                UserId = userId,
                                Name = userName,
                                Orders = new List<OrderDto>()
                            };
                            usersMap[userId] = user;
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("OrderId")))
                        {
                            user.Orders.Add(new OrderDto()
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                Total = reader.GetDecimal(reader.GetOrdinal("OrderTotal"))
                            });
                        }
                    }
                    //Если у пользователя есть заказ (!NULL) 

                }

                // Возвращаем список пользователей
                return usersMap.Values.ToList();
            }
        } 
    }
}