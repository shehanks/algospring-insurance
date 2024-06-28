﻿using AlgospringInsurance.App.Dtos;
using AlgospringInsurance.DataAccess.UnitOfWork;
using AlgospringInsurance.Infrastructure;
using System.Collections.Immutable;

namespace AlgospringInsurance.App.Forms
{
    public partial class AdminForm : Form
    {
        private readonly IUnitOfWork unitOfWork;

        private readonly IValidationProvider validationProvider;

        public AdminForm(
            IUnitOfWork unitOfWork,
            IValidationProvider validationProvider)
        {
            this.unitOfWork = unitOfWork;
            this.validationProvider = validationProvider;
            InitializeComponent();
            LoadUserNameItems();
        }

        #region Control Events

        private void AdminForm_Search_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedUser = AdminForm_Search_ComboBox.SelectedItem as DropDownItem;

            AdminForm_UseRegistration_Register_Button.Enabled = false;
            AdminForm_UseRegistration_Update_Button.Enabled = true;
            AdminForm_UseRegistration_Delete_Button.Enabled = true;

            ResetErrorProviders();

            try
            {
                var user = unitOfWork.UserRepository.GetById(selectedUser!.Id)!;

                AdminForm_UseRegistration_Email_TextBox.Text = user.Email;
                AdminForm_UseRegistration_Name_TextBox.Text = user.Name;
                AdminForm_UseRegistration_Username_TextBox.Text = user.Username;
                AdminForm_UseRegistration_IsAdmin_CheckBox.Checked = user.IsAdmin;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Unexpected error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdminForm_UseRegistration_Register_Button_Click(object sender, EventArgs e)
        {
            if (!IsValidUser())
                return;

            try
            {
                var user = unitOfWork.UserRepository.Insert(new DataAccess.Models.User
                {
                    Name = AdminForm_UseRegistration_Name_TextBox.Text.Trim(),
                    Email = AdminForm_UseRegistration_Email_TextBox.Text.Trim(),
                    Username = AdminForm_UseRegistration_Username_TextBox.Text.Trim(),
                    Password = SecurityProvider.Encrypt(AdminForm_UseRegistration_Password_TextBox.Text),
                    IsAdmin = AdminForm_UseRegistration_IsAdmin_CheckBox.Checked
                });

                unitOfWork.Complete();

                MessageBox.Show("Record Added Succesfully", "Register User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetUserForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdminForm_UseRegistration_Update_Button_Click(object sender, EventArgs e)
        {
            var selectedUser = AdminForm_Search_ComboBox.SelectedItem as DropDownItem;

            if (!IsValidUser())
                return;

            try
            {
                var user = unitOfWork.UserRepository.GetById(selectedUser!.Id);

                if (user is not null)
                {
                    user.Name = AdminForm_UseRegistration_Name_TextBox.Text.Trim();
                    user.Email = AdminForm_UseRegistration_Email_TextBox.Text.Trim();
                    user.Username = AdminForm_UseRegistration_Username_TextBox.Text.Trim();
                    user.IsAdmin = AdminForm_UseRegistration_IsAdmin_CheckBox.Checked;

                    if (!string.IsNullOrWhiteSpace(AdminForm_UseRegistration_Password_TextBox.Text))
                        user.Password = SecurityProvider.Encrypt(AdminForm_UseRegistration_Password_TextBox.Text);

                    unitOfWork.UserRepository.Update(user);
                    unitOfWork.Complete();

                    MessageBox.Show("Record Updated Succesfully", "Update User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetUserForm();
                }
                else
                {
                    MessageBox.Show("Invalid record", "Update User",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdminForm_UseRegistration_Delete_Button_Click(object sender, EventArgs e)
        {
            var selectedUser = AdminForm_Search_ComboBox.SelectedItem as DropDownItem;

            try
            {

                var user = unitOfWork.UserRepository.GetById(selectedUser!.Id);

                if (user is not null)
                {
                    var deleteDialog = MessageBox.Show(
                        "Are you sure, Do you really want to Delete this Record...?",
                        "Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (deleteDialog == DialogResult.Yes)
                    {
                        unitOfWork.UserRepository.Delete(user);
                        unitOfWork.Complete();
                        MessageBox.Show("Record Deleted Succesfully", "Delete user", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetUserForm();
                    }
                }
                else
                {
                    MessageBox.Show("Invalid record", "Delete User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdminForm_UseRegistration_Reset_Button_Click(object sender, EventArgs e) => ResetUserForm();

        private void AdminForm_UseRegistration_Name_TextBox_TextChanged(object sender, EventArgs e) => ValidateName();

        private void AdminForm_UseRegistration_Email_TextBox_TextChanged(object sender, EventArgs e) => ValidateEmail();

        private void AdminForm_UseRegistration_Username_TextBox_TextChanged(object sender, EventArgs e) => ValidateUsername();

        private void AdminForm_UseRegistration_Password_TextBox_TextChanged(object sender, EventArgs e) => ValidatePassword();

        #endregion

        #region Support Functions

        private void LoadUserNameItems()
        {
            AdminForm_Search_ComboBox.Items.Clear();

            try
            {
                var users = unitOfWork.UserRepository
                    .Get(u => !string.Equals(u.Email, LoginFormDataParser.Email));

                if (users.Any())
                {
                    foreach (var user in users)
                        AddNewUserSearchComboBoxItem(user.Id, user.Name, user.Email);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetUserForm()
        {
            LoadUserNameItems();
            AdminForm_UseRegistration_Name_TextBox.ResetText();
            AdminForm_UseRegistration_Email_TextBox.ResetText();
            AdminForm_UseRegistration_Password_TextBox.ResetText();
            AdminForm_UseRegistration_Username_TextBox.ResetText();
            AdminForm_UseRegistration_IsAdmin_CheckBox.Checked = false;

            ResetErrorProviders();

            AdminForm_UseRegistration_Register_Button.Enabled = true;
            AdminForm_UseRegistration_Update_Button.Enabled = false;
        }

        private void AddNewUserSearchComboBoxItem(int id, string name, string email) =>
            AdminForm_Search_ComboBox.Items.Add(new DropDownItem
            {
                Id = id,
                Text = $"{name} - {email}"
            });

        private bool isEditMode() => AdminForm_Search_ComboBox.SelectedItem as DropDownItem is not null;

        private void ResetErrorProviders() =>
            ImmutableList.Create(
                AdminForm_UseRegistration_Name_ErrorProvider,
                AdminForm_UseRegistration_Email_ErrorProvider,
                AdminForm_UseRegistration_Username_ErrorProvider,
                AdminForm_UseRegistration_Password_ErrorProvider)
                .ForEach(x => x.Clear());

        #endregion

        #region Validations

        private bool IsValidUser() =>
            validationProvider.ValidateAll(new List<Func<bool>>()
            {
                () => ValidateName(),
                () => ValidateEmail(),
                () => ValidateUsername(),
                () => ValidatePassword()
            });

        private bool ValidateName() =>
            validationProvider.Required(AdminForm_UseRegistration_Name_TextBox, AdminForm_UseRegistration_Name_ErrorProvider);

        private bool ValidateEmail() =>
            validationProvider.Required(AdminForm_UseRegistration_Email_TextBox, AdminForm_UseRegistration_Email_ErrorProvider) &&
            validationProvider.Email(AdminForm_UseRegistration_Email_TextBox, AdminForm_UseRegistration_Email_ErrorProvider);

        private bool ValidateUsername() =>
            validationProvider.Required(AdminForm_UseRegistration_Username_TextBox, AdminForm_UseRegistration_Username_ErrorProvider) &&
            validationProvider.Length(4, AdminForm_UseRegistration_Username_TextBox, AdminForm_UseRegistration_Username_ErrorProvider);

        private bool ValidatePassword()
        {
            if (!isEditMode() ||
                (isEditMode() && !string.IsNullOrEmpty(AdminForm_UseRegistration_Password_TextBox.Text)))
            {
                return
                    validationProvider.Required(AdminForm_UseRegistration_Password_TextBox, AdminForm_UseRegistration_Password_ErrorProvider) &&
                    validationProvider.Length(3, AdminForm_UseRegistration_Password_TextBox, AdminForm_UseRegistration_Password_ErrorProvider);
            }

            AdminForm_UseRegistration_Password_ErrorProvider.Clear();
            return true;
        }

        #endregion
    }
}
