using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;

namespace Foodie.Admin
{
    public partial class Category : System.Web.UI.Page
    {
        private MySqlConnection con;
        private MySqlCommand cmd;
        private MySqlDataAdapter sda;
        private DataTable dt;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session["breadCrum"] = "Category";
                getCategories();
            }
            lblMsg.Visible = false;
        }

        protected void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            string actionName = string.Empty, imagePath = string.Empty, fileExtension = string.Empty;
            bool isValidToExecute = false;
            int categoryId = Convert.ToInt32(hdnId.Value);

            con = new MySqlConnection(Connection.GetConnectionString());
            cmd = new MySqlCommand("Category_Crud", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", categoryId == 0 ? "INSERT" : "UPDATE");
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
            cmd.Parameters.AddWithValue("@IsActive", cbIsActive.Checked);

            if (fuCategoryImage.HasFile)
            {
                if (Utils.IsValidExtension(fuCategoryImage.FileName))
                {
                    Guid obj = Guid.NewGuid();
                    fileExtension = Path.GetExtension(fuCategoryImage.FileName);
                    imagePath = "Images/Category/" + obj.ToString() + fileExtension;
                    fuCategoryImage.PostedFile.SaveAs(Server.MapPath("~/Images/Category/") + obj.ToString() + fileExtension);
                    cmd.Parameters.AddWithValue("@ImageUrl", imagePath);
                    isValidToExecute = true;
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "Please select a .jpg, .jpeg, or .png image";
                    lblMsg.CssClass = "alert alert-danger";
                    isValidToExecute = false;
                }
            }
            else if (categoryId > 0) // For update, if no new image is selected, maintain existing value
            {
                cmd.Parameters.AddWithValue("@ImageUrl", DBNull.Value);
                isValidToExecute = true;
            }
            else
            {
                cmd.Parameters.AddWithValue("@ImageUrl", DBNull.Value);
                isValidToExecute = true;
            }

            if (isValidToExecute)
            {
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    actionName = categoryId == 0 ? "inserted" : "updated";
                    lblMsg.Visible = true;
                    lblMsg.Text = "Category " + actionName + " successfully!";
                    lblMsg.CssClass = "alert alert-success";
                    getCategories();
                    clear();
                }
                catch (Exception ex)
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "Error - " + ex.Message;
                    lblMsg.CssClass = "alert alert-danger";
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private void getCategories()
        {
            con = new MySqlConnection(Connection.GetConnectionString());
            cmd = new MySqlCommand("Category_Crud", con);
            cmd.CommandType = CommandType.StoredProcedure;

            // Add the required parameters for the SELECT action
            cmd.Parameters.AddWithValue("@Action", "SELECT");
            cmd.Parameters.AddWithValue("@CategoryId", 0);  // Use 0 to get all categories
            cmd.Parameters.AddWithValue("@Name", DBNull.Value);  // Pass DBNull.Value if not required
            cmd.Parameters.AddWithValue("@IsActive", DBNull.Value); // Pass DBNull.Value if not required
            cmd.Parameters.AddWithValue("@ImageUrl", DBNull.Value); // Pass DBNull.Value if not required

            sda = new MySqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);

            rCategory.DataSource = dt;
            rCategory.DataBind();
            con.Close();
        }



        private void clear()
        {
            txtName.Text = string.Empty;
            cbIsActive.Checked = false;
            hdnId.Value = "0";
            btnAddOrUpdate.Text = "Add";
            imgCategory.ImageUrl = "../Images/No_image.png";
            imgCategory.Height = 0;
            imgCategory.Width = 0;
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            clear();
        }

        protected void rCategory_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            lblMsg.Visible = false;
            MySqlConnection con = new MySqlConnection(Connection.GetConnectionString());

            try
            {
                // Edit functionality
                if (e.CommandName == "edit")
                {
                    using (con)
                    using (cmd = new MySqlCommand("Category_Crud", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Action", "GETBYID");
                        cmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt32(e.CommandArgument));

                        MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];
                            txtName.Text = row["Name"].ToString();
                            cbIsActive.Checked = Convert.ToBoolean(row["IsActive"]);
                            string imageUrl = row["ImageUrl"].ToString();
                            imgCategory.ImageUrl = string.IsNullOrEmpty(imageUrl) ? "../Images/No_image.png" : "../" + imageUrl;
                            imgCategory.Height = 200;
                            imgCategory.Width = 200;
                            hdnId.Value = row["CategoryId"].ToString();
                            btnAddOrUpdate.Text = "Update";
                        }
                    }
                }
                // Delete functionality
                else if (e.CommandName == "delete")
                {
                    using (con)
                    using (cmd = new MySqlCommand("Category_Crud", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Action", "DELETE");
                        cmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt32(e.CommandArgument));

                        con.Open();
                        cmd.ExecuteNonQuery();

                        lblMsg.Visible = true;
                        lblMsg.Text = "Category deleted successfully";
                        lblMsg.CssClass = "alert alert-success";
                        getCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error - " + ex.Message;
                lblMsg.CssClass = "alert alert-danger";
            }
            finally
            {
                con.Close();
            }
        }


        protected void rCategory_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Optional logic when each item is bound
            // For example:
            // if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            // {
            //     Label lbl = (Label)e.Item.FindControl("lblCategoryName");
            //     // Do something with the label
            // }
        }
    }
}
