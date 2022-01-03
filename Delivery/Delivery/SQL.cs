using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using static Delivery.GlobalVar;

namespace Delivery
{
    public static class SQL
    {
        public static bool CheckLogin(string surname, string password)
        {
            
            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "SELECT Id FROM Staff WHERE Surname='" + surname + "' AND Password='" + password + "'",
                    Connection = connection
                };

                object result = cmd.ExecuteScalar();

                if (result == DBNull.Value || result == null)
                {
                    
                    return false;

                }
                else
                {
                    
                    return true;
                }
            }
           
        }

        public static void UpdateOrder(int orderId, string toChange, string newValue)
        {
            using(SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "UPDATE Orders SET " + toChange + "='" + newValue + "' WHERE Id='" + orderId + "'",
                    Connection = conn
                };

                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateCustomer(string customerEmail, string toSet, string newValue)
        {
            using(SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "UPDATE Customers SET " + toSet + "='" + newValue + "' WHERE Email='" + customerEmail + "'",
                    Connection = conn
                };

                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateCustomerOrdersAmount(string email)
        {
            using (SqlConnection connectionCheck = new SqlConnection(_ConnectionString))
            {
                connectionCheck.Open();

                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "UPDATE Customers SET Orders=Orders+1 WHERE Email='" + email + "'",

                    Connection = connectionCheck
                };



                cmd.ExecuteNonQuery();


            }
        }

        public static void UpdateCustomerTotalSpending(string email, string spending)
        {
            using (SqlConnection connectionCheck = new SqlConnection(_ConnectionString))
            {
                connectionCheck.Open();

                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "UPDATE Customers SET TotalSpending=TotalSpending+'" + spending + "' WHERE Email='" + email + "'",

                    Connection = connectionCheck
                };



                cmd.ExecuteNonQuery();


            }
        }

        public static void UpdateOrder(int orderId, string toChange, string newValue, string staff) // UPDATE ORDER LOCATION
        {
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "UPDATE Orders SET " + toChange + "='" + newValue + "' WHERE Id='" + orderId + "' AND Staff='" + staff + "'",
                    Connection = conn
                };

                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateCurrentStaffOrdersLocation(double latitude, double longtitude, string staff)
        {
            using(SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "UPDATE Orders SET LocationLat='" + latitude + "', LocationLon='" + longtitude + "' WHERE Staff='" + staff + "' AND IsActive='True'",
                    Connection = conn
                };

                cmd.ExecuteNonQuery();
            }
        }

        public static List<Staff> GetStaffDetails(string surname)
        {
            List<Staff> staffDetail = new List<Staff>();
            using(SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "SELECT * FROM Staff WHERE Surname='" + surname + "'",
                    Connection = conn
                };
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    staffDetail.Add(new Staff
                    {
                        FirstName = reader["FirstName"].ToString(),
                        Surname = reader["Surname"].ToString(),
                        Department = reader["Department"].ToString(),
                        Salary = decimal.Parse(reader["Salary"].ToString()),
                        Contract = reader["Contract"].ToString(),
                        Image = reader["Image"].ToString()
                    });
                }
                
            }
            return staffDetail;
        }
    }
}
