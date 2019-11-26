using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using AutoLotDAL.Models;

namespace AutoLotDAL.DataOperations
{
    public class InventoryDAL
    {
        private readonly string _connectionString;
        private SqlConnection _sqlConnection = null;

        public InventoryDAL() : this(
            @"Data Source = (localdb)\mssqllocaldb;Integrated Security=true;Initial Catalog=Northwind")
        {
        }

        public InventoryDAL(string connectionString)
            => _connectionString = connectionString;

        private void OpenConnection()
        {
            _sqlConnection = new SqlConnection {ConnectionString = _connectionString};
            _sqlConnection.Open();
        }

        private void CloseConnection()
        {
            if (_sqlConnection?.State != ConnectionState.Closed)
            {
                _sqlConnection?.Close();
            }
        }

        public List<Shipper> GetAllInventory()
        {
            OpenConnection();
            // This will hold the records.
            List<Shipper> inventory = new List<Shipper>();

            // Prep command object.
            string sql = "Select * From Shippers";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    inventory.Add(new Shipper
                    {
                        ShipperID = (int) dataReader["ShipperID"],
                        CompanyName = (string) dataReader["CompanyName"],
                        Phone = (string) dataReader["Phone"]
                    });
                }
                dataReader.Close();
            }
            return inventory;
        }
        /*
        public Shipper GetShipper(int id)
        {
            OpenConnection();
            Shipper Shipper = null;
            string sql = $"Select * From Inventory where ShipperId = {id}";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    Shipper = new Shipper
                    {
                        ShipperID = (int) dataReader["ShipperID"],
                        CompanyName = (string) dataReader["CompanyName"],
                        Phone = (string) dataReader["Phone"]
                    };
                }
                dataReader.Close();
            }
            return Shipper;
        }

        public void InsertAuto(string color, string make, string petName)
        {
            OpenConnection();
            // Format and execute SQL statement.
            string sql = $"Insert Into Inventory (Make, Color, PetName) Values ('{make}', '{color}', '{petName}')";

            // Execute using our connection.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }
        //public void InsertAuto(Shipper Shipper)
        //{
        //    OpenConnection();
        //    // Format and execute SQL statement.
        //    string sql = "Insert Into Inventory (Make, Color, PetName) Values " +
        //        $"('{Shipper.Make}', '{Shipper.Color}', '{Shipper.PetName}')";

        //    // Execute using our connection.
        //    using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
        //    {
        //        command.CommandType = CommandType.Text;
        //        command.ExecuteNonQuery();
        //    }
        //    CloseConnection();
        //}
        public void InsertAuto(Shipper Shipper)
        {
            OpenConnection();
            // Note the "placeholders" in the SQL query.
            string sql = "Insert Into Inventory" +
                         "(Make, Color, PetName) Values" +
                         "(@Make, @Color, @PetName)";

            // This command will have internal parameters.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                // Fill params collection.
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@Make",
                    Value = Shipper.Make,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@Color",
                    Value = Shipper.Color,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@PetName",
                    Value = Shipper.PetName,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                command.Parameters.Add(parameter);

                command.ExecuteNonQuery();
                CloseConnection();
            }
        }

        public void DeleteShipper(int id)
        {
            OpenConnection();
            // Get ID of Shipper to delete, then do so.
            string sql = $"Delete from Inventory where ShipperId = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                try
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! That Shipper is on order!", ex);
                    throw error;
                }
            }
            CloseConnection();
        }

        public void UpdateShipperPetName(int id, string newPetName)
        {
            OpenConnection();
            // Get ID of Shipper to modify the pet name.
            string sql = $"Update Inventory Set PetName = '{newPetName}' Where ShipperId = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }

        public string LookUpPetName(int ShipperId)
        {
            OpenConnection();
            string ShipperPetName;

            // Establish name of stored proc.
            using (SqlCommand command = new SqlCommand("GetPetName", _sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Input param.
                SqlParameter param = new SqlParameter
                {
                    ParameterName = "@ShipperId",
                    SqlDbType = SqlDbType.Int,
                    Value = ShipperId,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(param);

                // Output param.
                param = new SqlParameter
                {
                    ParameterName = "@petName",
                    SqlDbType = SqlDbType.Char,
                    Size = 10,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(param);

                // Execute the stored proc.
                command.ExecuteNonQuery();

                // Return output param.
                ShipperPetName = (string) command.Parameters["@petName"].Value;
                CloseConnection();
            }
            return ShipperPetName;
        }

        public void ProcessCreditRisk(bool throwEx, int custId)
        {
            OpenConnection();
            // First, look up current name based on customer ID.
            string fName;
            string lName;
            var cmdSelect =
                new SqlCommand($"Select * from Customers where CustId = {custId}",
                    _sqlConnection);
            using (var dataReader = cmdSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fName = (string) dataReader["FirstName"];
                    lName = (string) dataReader["LastName"];
                }
                else
                {
                    CloseConnection();
                    return;
                }
            }

            // Create command objects that represent each step of the operation.
            var cmdRemove =
                new SqlCommand($"Delete from Customers where CustId = {custId}",
                    _sqlConnection);

            var cmdInsert =
                new SqlCommand("Insert Into CreditRisks" +
                               $"(FirstName, LastName) Values('{fName}', '{lName}')",
                    _sqlConnection);

            // We will get this from the connection object.
            SqlTransaction tx = null;
            try
            {
                tx = _sqlConnection.BeginTransaction();

                // Enlist the commands into this transaction.
                cmdInsert.Transaction = tx;
                cmdRemove.Transaction = tx;

                // Execute the commands.
                cmdInsert.ExecuteNonQuery();
                cmdRemove.ExecuteNonQuery();

                // Simulate error.
                if (throwEx)
                {
                    throw new Exception("Sorry!  Database error! Tx failed...");
                }

                // Commit it!
                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Any error will roll back transaction.  Using the new conditional access operator to check for null.
                tx?.Rollback();
            }
            finally
            {
                CloseConnection();
            }
        }
        */
    }
}