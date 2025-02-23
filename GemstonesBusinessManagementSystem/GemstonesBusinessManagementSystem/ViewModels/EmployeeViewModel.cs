﻿using ClosedXML.Excel;
using GemstonesBusinessManagementSystem.DAL;
using GemstonesBusinessManagementSystem.Models;
using GemstonesBusinessManagementSystem.Resources.UserControls;
using GemstonesBusinessManagementSystem.Views;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GemstonesBusinessManagementSystem.ViewModels
{
    class EmployeeViewModel : BaseViewModel
    {
        //EmployeePositionControl
        public ICommand DeletePositionCommand { get; set; }
        public ICommand ViewPositionCommand { get; set; }

        //EmployeePositionWindow
        public ICommand ClearViewCommand { get; set; }
        public ICommand LoadPositionCommand { get; set; }
        public ICommand AddPositionCommand { get; set; }
        public ICommand OpenPositionWindowCommand { get; set; }
        public ICommand SeparateThousandsCommand { get; set; }

        //EmployeeControl
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        //AddEmployeeWindow
        public ICommand SelectImageCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        //MainWindow
        public ICommand FilterCommand { get; set; }
        public ICommand SortCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        public ICommand NextPageCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        public ICommand OpenAddWindowCommand { get; set; }
        public ICommand LoadCommand { get; set; }

        private int currentPage = 0;
        Binding newBinding;
        private string standard;

        //Mainwindow
        private MainWindow mainWindow;

        private List<Employee> employeeList = EmployeeDAL.Instance.GetList();
        private EmployeeControl employeeControl;

        //PositionWindow
        private EmployeePositionWindow employeePositionWindow;
        public bool isEditingPosition = false;
        private EmployeePositionControl empPosControl;

        private EmployeePosition selectedPosition;
        public EmployeePosition SelectedPosition { get => selectedPosition; set { selectedPosition = value; OnPropertyChanged(); } }
        private EmployeePosition filterPosition;
        public EmployeePosition FilterPosition { get => filterPosition; set { filterPosition = value; OnPropertyChanged(); } }

        private ObservableCollection<EmployeePosition> itemSourcePosition = new ObservableCollection<EmployeePosition>();
        public ObservableCollection<EmployeePosition> ItemSourcePosition { get => itemSourcePosition; set { itemSourcePosition = value; OnPropertyChanged(); } }
        private ObservableCollection<EmployeePosition> itsAddEmpPosition = new ObservableCollection<EmployeePosition>();
        public ObservableCollection<EmployeePosition> ItsAddEmpPosition { get => itsAddEmpPosition; set { itsAddEmpPosition = value; OnPropertyChanged(); } }

        public string SalaryBase { get => salaryBase; set => salaryBase = value; }
        public string MoneyPerShift { get => moneyPerShift; set => moneyPerShift = value; }
        public string MoneyPerFault { get => moneyPerFault; set => moneyPerFault = value; }
        public string Standard { get => standard; set => standard = value; }
        public string Address { get => address; set => address = value; }
        public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
        public string Name { get => name; set => name = value; }

        public string imageName;
        public bool isEditing = false;
        private string oldPosition;
        private string salaryBase;
        private string moneyPerShift;
        private string moneyPerFault;
        private string address;
        private string phoneNumber;
        private string name;

        public EmployeeViewModel()
        {
            //EmployeePositionControl
            DeletePositionCommand = new RelayCommand<EmployeePositionControl>((p) => true, (p) => DeletePosition(p));
            ViewPositionCommand = new RelayCommand<EmployeePositionControl>((p) => true, (p) => ViewPosition(p));

            //EmployeePositionWindow
            ClearViewCommand = new RelayCommand<EmployeePositionWindow>((p) => true, (p) => ClearView(p));
            LoadPositionCommand = new RelayCommand<EmployeePositionWindow>((p) => true, (p) => LoadPosition(p));
            AddPositionCommand = new RelayCommand<EmployeePositionWindow>((p) => true, (p) => AddOrUpdatePosition(p));
            OpenPositionWindowCommand = new RelayCommand<MainWindow>((p) => true, (p) => OpenPositionWindow(p));
            SeparateThousandsCommand = new RelayCommand<TextBox>((parameter) => true, (parameter) => SeparateThousands(parameter));

            //EmployeeControl
            EditCommand = new RelayCommand<EmployeeControl>((p) => true, (p) => OpenEditWindow(p));
            DeleteCommand = new RelayCommand<EmployeeControl>((p) => true, (p) => HandleDelete(p));

            //AddEmployeeWindow
            SelectImageCommand = new RelayCommand<AddEmployeeWindow>((p) => true, (p) => HandleSelectImage(p));
            SaveCommand = new RelayCommand<AddEmployeeWindow>((p) => true, (p) => HandleAddOrUpdate(p));
            ExitCommand = new RelayCommand<AddEmployeeWindow>((p) => true, (p) => p.Close());

            //MainWindow
            FilterCommand = new RelayCommand<MainWindow>((p) => true, (p) => Filter(p));
            SortCommand = new RelayCommand<MainWindow>((p) => true, (p) => Sort(p));
            PreviousPageCommand = new RelayCommand<MainWindow>((p) => true, (p) => GoToPreviousPage(p, --currentPage));
            NextPageCommand = new RelayCommand<MainWindow>((p) => true, (p) => GoToNextPage(p, ++currentPage));
            SearchCommand = new RelayCommand<MainWindow>((p) => true, (p) => Search(p));
            ExportExcelCommand = new RelayCommand<MainWindow>((p) => true, (p) => ExportExcel(p));
            OpenAddWindowCommand = new RelayCommand<MainWindow>((p) => true, (p) => OpenAddWindow(p));
            LoadCommand = new RelayCommand<MainWindow>((p) => true, (p) => { LoadEmployeeList(p, 0); SetItemSource(); });
        }

        //EmployeePositionControl
        void ClearView(EmployeePositionWindow window)
        {
            if (newBinding != null)
            {
                window.txtPosition.SetBinding(TextBox.TextProperty, newBinding);
            }
            isEditingPosition = false;
            window.txtId.Text = AddPrefix("CV", (EmployeePositionDAL.Instance.GetMaxId() + 1));
            window.txtPosition.Text = null;
            window.txtSalaryBase.Text = null;
            window.txtStandardWorkDays.Text = null;
            window.txtOvertime.Text = null;
            window.txtFault.Text = null;
            window.btnSave.Content = "Lưu";
            window.btnSave.ToolTip = "Lưu";
            window.txbTitle.Text = "Thêm chức vụ";
        }
        void ViewPosition(EmployeePositionControl control)
        {
            Binding binding = BindingOperations.GetBinding(employeePositionWindow.txtPosition, TextBox.TextProperty);
            if (binding != null)
            {
                newBinding = CloneBinding(binding as BindingBase, binding.Source) as Binding;
            }
            BindingOperations.ClearBinding(employeePositionWindow.txtPosition, TextBox.TextProperty);
            oldPosition = control.txbPosition.Text;
            empPosControl = control;
            isEditingPosition = true;
            employeePositionWindow.txbTitle.Text = "Sửa chức vụ";
            employeePositionWindow.btnSave.Content = "Cập nhật";
            employeePositionWindow.btnSave.ToolTip = "Cập nhật";
            employeePositionWindow.txtId.Text = control.txbId.Text;

            employeePositionWindow.txtPosition.Text = control.txbPosition.Text;
            employeePositionWindow.txtPosition.SelectionStart = control.txbPosition.Text.Length;

            employeePositionWindow.txtSalaryBase.Text = control.txbSalaryBase.Text;
            employeePositionWindow.txtSalaryBase.SelectionLength = control.txbSalaryBase.Text.Length;

            employeePositionWindow.txtStandardWorkDays.Text = control.txbWorkdays.Text;
            employeePositionWindow.txtStandardWorkDays.SelectionLength = control.txbWorkdays.Text.Length;

            employeePositionWindow.txtOvertime.Text = control.txbShift.Text;
            employeePositionWindow.txtOvertime.SelectionLength = control.txbShift.Text.Length;

            employeePositionWindow.txtFault.Text = control.txbFault.Text;
            employeePositionWindow.txtFault.SelectionLength = control.txbFault.Text.Length;
        }
        void DeletePosition(EmployeePositionControl control)
        {
            string idPosition = ConvertToIDString(control.txbId.Text);
            if (EmployeeDAL.Instance.IsPosition(idPosition))
            {
                CustomMessageBox.Show("Không thể xóa vì tồn tại nhân viên có chức vụ này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBoxResult result = CustomMessageBox.Show("Xác nhận xóa chức vụ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool isSuccess = EmployeePositionDAL.Instance.Delete(idPosition);
                    if (isSuccess)
                    {
                        employeePositionWindow.stkPosition.Children.Remove(control);
                    }
                    else
                    {
                        CustomMessageBox.Show("Xoá thất bại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        //EmployeePositionWindow
        void LoadPosition(EmployeePositionWindow window)
        {
            employeePositionWindow = window;

            List<EmployeePosition> positions = EmployeePositionDAL.Instance.GetList();
            window.stkPosition.Children.Clear();

            foreach (var position in positions)
            {
                EmployeePositionControl control = new EmployeePositionControl();
                control.txbId.Text = AddPrefix("CV", position.IdEmployeePosition);
                control.txbPosition.Text = position.Position;
                control.txbSalaryBase.Text = SeparateThousands(position.SalaryBase.ToString());
                control.txbShift.Text = position.MoneyPerShift.ToString();
                control.txbFault.Text = position.MoneyPerFault.ToString();
                control.txbWorkdays.Text = position.StandardWorkDays.ToString();

                window.stkPosition.Children.Add(control);
            }
        }
        void AddOrUpdatePosition(EmployeePositionWindow window)
        {
            #region
            if (string.IsNullOrEmpty(window.txtPosition.Text))
            {
                CustomMessageBox.Show("Vui lòng nhập chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtPosition.Focus();
                return;
            }
            if (string.IsNullOrEmpty(window.txtSalaryBase.Text))
            {
                CustomMessageBox.Show("Vui lòng nhập lương cơ bản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtSalaryBase.Focus();
                return;
            }
            if (string.IsNullOrEmpty(window.txtStandardWorkDays.Text))
            {
                CustomMessageBox.Show("Vui lòng nhập ngày công chuẩn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtStandardWorkDays.Focus();
                return;
            }
            if (string.IsNullOrEmpty(window.txtOvertime.Text))
            {
                CustomMessageBox.Show("Vui lòng nhập tiền lương tăng ca!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtOvertime.Focus();
                return;
            }
            if (string.IsNullOrEmpty(window.txtFault.Text))
            {
                CustomMessageBox.Show("Vui lòng nhập tiền phạt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtFault.Focus();
                return;
            }
            #endregion
            if(!string.IsNullOrEmpty(oldPosition))
            {
                if (window.txtPosition.Text.ToLower() != oldPosition.ToLower() && EmployeePositionDAL.Instance.IsExisted(window.txtPosition.Text))
                {
                    CustomMessageBox.Show("Chức vụ đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.txtPosition.Focus();
                    return;
                }
            }
            if (string.Compare(window.txtStandardWorkDays.Text, "30") > 0)
            {
                CustomMessageBox.Show("Số ngày công chuẩn không > 30!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtStandardWorkDays.Focus();
                return;
            }
            EmployeePosition position = new EmployeePosition(ConvertToID(window.txtId.Text), window.txtPosition.Text,
                ConvertToNumber(window.txtSalaryBase.Text), ConvertToNumber(window.txtOvertime.Text),
                ConvertToNumber(window.txtFault.Text), int.Parse(window.txtStandardWorkDays.Text));

            EmployeePositionDAL.Instance.InsertOrUpdate(position, isEditingPosition);
            if (isEditingPosition)
            {
                empPosControl.txbId.Text = window.txtId.Text;
                empPosControl.txbPosition.Text = window.txtPosition.Text;
                empPosControl.txbSalaryBase.Text = window.txtSalaryBase.Text;
                empPosControl.txbWorkdays.Text = window.txtStandardWorkDays.Text;
                empPosControl.txbShift.Text = window.txtOvertime.Text;
                empPosControl.txbFault.Text = window.txtFault.Text;

                window.txbTitle.Text = "Thêm chức vụ";
                employeePositionWindow.btnSave.Content = "Lưu";
                employeePositionWindow.btnSave.ToolTip = "Lưu";
            }
            else
            {
                EmployeePositionControl control = new EmployeePositionControl();
                control.txbId.Text = window.txtId.Text;
                control.txbPosition.Text = position.Position;
                control.txbSalaryBase.Text = SeparateThousands(position.SalaryBase.ToString());
                control.txbShift.Text = position.MoneyPerShift.ToString();
                control.txbFault.Text = position.MoneyPerFault.ToString();
                control.txbWorkdays.Text = position.StandardWorkDays.ToString();

                window.stkPosition.Children.Add(control);

                for (int i = 1; i <= 13; i++)
                {
                    PositionDetail positionDetail = new PositionDetail(ConvertToID(window.txtId.Text), i, false);
                    PositionDetailDAL.Instance.InsertOrUpdate(positionDetail);
                }
            }
            SetItemSource();
            LoadEmployeeList(mainWindow, 0);
            int start = 0, end = 0;
            LoadInfoOfPage(ref start, ref end);
            isEditingPosition = false;
            ClearView(window);
        }
        void OpenPositionWindow(MainWindow mainWindow)
        {
            EmployeePositionWindow window = new EmployeePositionWindow();
            window.txtId.Text = AddPrefix("CV", (EmployeePositionDAL.Instance.GetMaxId() + 1));
            window.txtPosition.Text = null;
            window.txtOvertime.Text = null;
            window.txtFault.Text = null;
            window.txtSalaryBase.Text = null;
            window.txtStandardWorkDays.Text = null;
            window.ShowDialog();
        }

        //EmployeeControl
        void OpenEditWindow(EmployeeControl control)
        {
            isEditing = true;
            Employee employee = EmployeeDAL.Instance.GetById(ConvertToIDString(control.txbId.Text));
            this.employeeControl = control;

            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.txtId.Text = control.txbId.Text;

            addEmployeeWindow.txtName.Text = employee.Name;
            addEmployeeWindow.txtName.SelectionStart = addEmployeeWindow.txtName.Text.Length;

            addEmployeeWindow.txtPhoneNumber.Text = employee.PhoneNumber;
            addEmployeeWindow.txtPhoneNumber.SelectionStart = addEmployeeWindow.txtPhoneNumber.Text.Length;

            addEmployeeWindow.txtAddress.Text = employee.Address;
            addEmployeeWindow.txtAddress.SelectionStart = addEmployeeWindow.txtAddress.Text.Length;

            addEmployeeWindow.cboPosition.Text = EmployeePositionDAL.Instance.GetById(employee.IdPosition).Position;

            if (employee.Gender == "Nam")
                addEmployeeWindow.rdoMale.IsChecked = true;
            else
                addEmployeeWindow.rdoFemale.IsChecked = true;
            addEmployeeWindow.dpBirthDate.SelectedDate = DateTime.Parse(employee.DateOfBirth.ToString());
            addEmployeeWindow.dpWorkDate.SelectedDate = DateTime.Parse(employee.StartingDate.ToString());
            ImageBrush imageBrush = new ImageBrush();
            if (employee.ImageFile != null)
            {
                imageBrush.ImageSource = Converter.Instance.ConvertByteToBitmapImage(employee.ImageFile);
                addEmployeeWindow.imgAvatar.Source = imageBrush.ImageSource;
            }
            else
            {
                addEmployeeWindow.imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/GemstonesBusinessManagementSystem;component/Resources/Images/employee.jpg"));
            }
            addEmployeeWindow.btnSave.ToolTip = "Cập nhật thông tin nhân viên";
            addEmployeeWindow.Title = "Cập nhật thông tin nhân viên";
            addEmployeeWindow.btnSave.Content = "Cập nhật";
            addEmployeeWindow.btnSave.ToolTip = "Cập nhật nhân viên";
            addEmployeeWindow.ShowDialog();
        }
        void HandleDelete(EmployeeControl employeeControl)
        {
            string idEmployee = ConvertToIDString(employeeControl.txbId.Text);
            MessageBoxResult result = CustomMessageBox.Show("Xác nhận xóa nhân viên?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool isSuccess = EmployeeDAL.Instance.Delete(idEmployee);
                if (isSuccess)
                {
                    mainWindow.stkEmployeeList.Children.Remove(employeeControl);
                    employeeList.RemoveAll(x => x.IdEmployee == int.Parse(idEmployee));
                    if (mainWindow.stkEmployeeList.Children.Count == 0 && currentPage != 0) // kiểm tra có hết trang để chuyển qua trang trước
                    {
                        LoadEmployeeList(mainWindow, currentPage - 1);
                    }
                    else
                    {
                        LoadEmployeeList(mainWindow, currentPage);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Xoá thất bại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            int start = 0, end = 0;
            LoadInfoOfPage(ref start, ref end);
        }

        //AddEmployeeWindow
        void HandleAddOrUpdate(AddEmployeeWindow window)
        {
            #region
            string gender = "";
            if (string.IsNullOrEmpty(window.txtName.Text))
            {
                CustomMessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtName.Focus();
                return;
            }
            if (window.cboPosition.Text == "")
            {
                CustomMessageBox.Show("Vui lòng nhập chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.cboPosition.Focus();
                return;
            }
            if (window.dpBirthDate.Text == "")
            {
                CustomMessageBox.Show("Vui lòng nhập ngày sinh!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.dpBirthDate.Focus();
                return;
            }
            else
            {
                DateTime dob = new DateTime();
                bool check = DateTime.TryParse(window.dpBirthDate.Text, out dob);
                if (!check)
                {
                    CustomMessageBox.Show("Vui lòng nhập lại ngày sinh vì ngày sinh không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.dpBirthDate.Focus();
                    return;
                }
            }
            if (window.dpWorkDate.Text == "")
            {
                CustomMessageBox.Show("Vui lòng nhập ngày vào làm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.dpWorkDate.Focus();
                return;
            }
            else
            {
                DateTime startDate = new DateTime();
                DateTime DOB = DateTime.Parse(window.dpBirthDate.Text);
                if (!DateTime.TryParse(window.dpWorkDate.Text, out startDate))
                {
                    CustomMessageBox.Show("Vui lòng nhập lại ngày vào làm vì ngày vào làm không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.dpWorkDate.Focus();
                    return;
                }
                if (CalculateAge(DOB, startDate) < 18)
                {
                    CustomMessageBox.Show("Vui lòng nhập lại ngày sinh vì chưa đủ 18 tuổi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.dpBirthDate.Focus();
                    return;
                }
                if (startDate < DOB)
                {
                    CustomMessageBox.Show("Vui lòng nhập lại ngày vào làm lớn hơn ngày sinh!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.dpWorkDate.Focus();
                    return;
                }
            }
            if (window.txtAddress.Text == "")
            {
                CustomMessageBox.Show("Vui lòng nhập địa chỉ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtAddress.Focus();
                return;
            }
            if (window.txtPhoneNumber.Text == "")
            {
                CustomMessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                window.txtPhoneNumber.Focus();
                return;
            }
            if (window.rdoMale.IsChecked.Value == true)
                gender = "Nam";
            else
                gender = "Nữ";

            string idEmployee = ConvertToIDString(window.txtId.Text);
            byte[] imgByteArr;
            imgByteArr = Converter.Instance.ConvertBitmapImageToBytes((BitmapImage)window.imgAvatar.Source);

            imageName = null;
            #endregion
            Employee employee = new Employee(int.Parse(idEmployee), window.txtName.Text, gender,
                window.txtPhoneNumber.Text, window.txtAddress.Text, DateTime.Parse(window.dpBirthDate.Text),
                SelectedPosition.IdEmployeePosition, DateTime.Parse(window.dpWorkDate.Text), 1, imgByteArr);

            EmployeeDAL.Instance.InsertOrUpdate(employee, isEditing);
            window.cboPosition.SelectedIndex = -1;
            window.Close();

            //Add usercontrol vào stackpanel
            if (isEditing)
            {
                this.employeeControl.txbName.Text = employee.Name.ToString();
                this.employeeControl.txbPosition.Text = EmployeePositionDAL.Instance.GetById(employee.IdPosition).Position;
                this.employeeControl.txbPhoneNumber.Text = employee.PhoneNumber.ToString();
                this.employeeControl.txbAddress.Text = employee.Address.ToString();
                if (FilterPosition != null && FilterPosition.IdEmployeePosition != employee.IdPosition)
                {
                    mainWindow.stkEmployeeList.Children.Remove(employeeControl);
                    employeeList.RemoveAll(x => x.IdEmployee == employee.IdEmployee);
                }
                else
                {
                    int i = employeeList.FindIndex(x => x.IdEmployee == employee.IdEmployee);
                    employeeList[i] = employee;
                }
                Filter(mainWindow);
                Sort(mainWindow);
            }
            else
            {
                EmployeeControl control = new EmployeeControl();
                EmployeePosition employeePosition = EmployeePositionDAL.Instance.GetById(employee.IdPosition);
                control.txbId.Text = window.txtId.Text;
                control.txbName.Text = employee.Name.ToString();
                control.txbPosition.Text = employeePosition.Position;
                control.txbTotalSalary.Text = SeparateThousands(employeePosition.SalaryBase.ToString());
                control.txbPhoneNumber.Text = employee.PhoneNumber.ToString();
                control.txbAddress.Text = employee.Address.ToString();

                if (FilterPosition == null || FilterPosition != null && FilterPosition.IdEmployeePosition == employee.IdPosition)
                {
                    employeeList.Add(employee);
                    if (currentPage == (employeeList.Count - 1) / 10)
                    {
                        mainWindow.stkEmployeeList.Children.Add(control);
                    }
                }
                Filter(mainWindow);
                Sort(mainWindow);
            }
            int start = 0, end = 0;
            LoadInfoOfPage(ref start, ref end);
        }
        public void SetPickedDay(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            try
            {
                datePicker.Text = ((DateTime)datePicker.SelectedDate).ToString();
            }
            catch
            {

            }
        }
        void HandleSelectImage(AddEmployeeWindow window)
        {
            OpenFileDialog op = new OpenFileDialog
            {
                Title = "Select a picture",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png"
            };
            if (op.ShowDialog() == true)
            {
                imageName = op.FileName;
                ImageBrush imageBrush = new ImageBrush();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imageName);
                bitmap.EndInit();
                imageBrush.ImageSource = bitmap;
                window.imgAvatar.Source = imageBrush.ImageSource;
            }
        }

        //MainWindow
        void SetItemSource()
        {
            int i = mainWindow.cboFilterPosition.SelectedIndex;

            itemSourcePosition.Clear();
            itsAddEmpPosition.Clear();

            EmployeePosition positionAll = new EmployeePosition(0, "Tất cả", 1, 1, 1, 1);
            itemSourcePosition.Add(positionAll);

            List<EmployeePosition> positions = EmployeePositionDAL.Instance.GetList();
            foreach (var position in positions)
            {
                itemSourcePosition.Add(position);
                itsAddEmpPosition.Add(position);
            }
            mainWindow.cboFilterPosition.SelectedIndex = i;
        }
        public void Search(MainWindow mainWindow)
        {
            mainWindow.cboSortEmployee.SelectedIndex = -1;
            mainWindow.cboFilterPosition.SelectedIndex = -1;
            string nameSearching = mainWindow.txtSearchEmployee.Text.ToLower();
            employeeList = EmployeeDAL.Instance.FindByName(nameSearching);
            currentPage = 0;
            LoadEmployeeList(mainWindow, currentPage);
        }
        public void ExportExcel(MainWindow main)
        {
            string filePath = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel |*.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
            }
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage p = new ExcelPackage())
                {
                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Danh sách nhân viên";
                    p.Workbook.Worksheets.Add("sheet");

                    ExcelWorksheet ws = p.Workbook.Worksheets[0];
                    ws.Name = "DSNCC";
                    ws.Cells.Style.Font.Size = 11;
                    ws.Cells.Style.Font.Name = "Calibri";
                    ws.Cells.Style.WrapText = true;
                    ws.Column(1).Width = 10;
                    ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(2).Width = 30;
                    ws.Column(3).Width = 20;
                    ws.Column(4).Width = 30;
                    ws.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(5).Width = 30;
                    ws.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(6).Width = 30;
                    ws.Column(7).Width = 30;
                    ws.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(8).Width = 30;
                    ws.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(9).Width = 30;
                    ws.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // Tạo danh sách các column header
                    string[] arrColumnHeader = { "STT", "Tên nhân viên", "Chức vụ", "Giới tính", "Ngày sinh", "Số điện thoại", "Địa chỉ", "Ngày vào làm", "Lương" };

                    var countColHeader = arrColumnHeader.Count();

                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge
                    ws.Row(1).Height = 15;
                    ws.Cells[1, 1].Value = "Danh sách nhân viên";
                    ws.Cells[1, 1, 1, countColHeader].Merge = true;

                    ws.Cells[1, 1, 1, countColHeader].Style.Font.Bold = true;
                    ws.Cells[1, 1, 1, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 2;
                    //tạo các header từ column header đã tạo từ bên trên
                    foreach (var item in arrColumnHeader)
                    {
                        ws.Row(rowIndex).Height = 15;
                        var cell = ws.Cells[rowIndex, colIndex];
                        //set màu 
                        var fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(255, 29, 161, 242);
                        cell.Style.Font.Bold = true;
                        //căn chỉnh các border
                        var border = cell.Style.Border;
                        border.Bottom.Style =
                            border.Top.Style =
                            border.Left.Style =
                            border.Right.Style = ExcelBorderStyle.Thin;

                        cell.Value = item;
                        colIndex++;
                    }
                    currentPage = 0;
                    // lấy ra danh sách nhà cung cấp
                    for (int i = 0; i < employeeList.Count; i++)
                    {
                        Employee employee = employeeList[i];
                        EmployeePosition employeePosition = EmployeePositionDAL.Instance.GetById(employeeList[i].IdPosition);
                        ws.Row(rowIndex).Height = 15;
                        colIndex = 1;
                        rowIndex++;
                        string address = "A" + rowIndex + ":I" + rowIndex;
                        ws.Cells[address].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        if (rowIndex % 2 != 0)
                        {
                            ws.Cells[address].Style.Fill.BackgroundColor.SetColor(255, 255, 255, 255);
                        }
                        else
                        {
                            ws.Cells[address].Style.Fill.BackgroundColor.SetColor(255, 229, 241, 255);
                        }

                        ws.Cells[rowIndex, colIndex++].Value = i + 1;
                        ws.Cells[rowIndex, colIndex++].Value = employee.Name;
                        ws.Cells[rowIndex, colIndex++].Value = employeePosition.Position;
                        ws.Cells[rowIndex, colIndex++].Value = employee.Gender;
                        ws.Cells[rowIndex, colIndex++].Value = employee.DateOfBirth.ToString("dd/MM/yyyy");
                        ws.Cells[rowIndex, colIndex++].Value = employee.PhoneNumber;
                        ws.Cells[rowIndex, colIndex++].Value = employee.Address;
                        ws.Cells[rowIndex, colIndex++].Value = employee.StartingDate.ToString("dd/MM/yyyy");
                        ws.Cells[rowIndex, colIndex++].Value = SeparateThousands(employeePosition.SalaryBase.ToString());
                        if (i % 10 == 9)
                        {
                            GoToNextPage(mainWindow, currentPage);
                        }
                    }
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                CustomMessageBox.Show("Xuất danh sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch
            {
            }

        }
        void OpenAddWindow(MainWindow mainWindow)
        {
            isEditing = false;
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.txtId.Text = AddPrefix("NV", (EmployeeDAL.Instance.GetMaxId() + 1));
            addEmployeeWindow.txtName.Text = null;
            addEmployeeWindow.txtAddress.Text = null;
            addEmployeeWindow.txtPhoneNumber.Text = null;
            addEmployeeWindow.cboPosition.SelectedIndex = -1;
            addEmployeeWindow.imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/GemstonesBusinessManagementSystem;component/Resources/Images/employee.jpg"));
            addEmployeeWindow.ShowDialog();
        }
        public void LoadEmployeeList(MainWindow main, int curPage)
        {
            this.mainWindow = main;
            mainWindow.stkEmployeeList.Children.Clear();

            int start = 0, end = 0;
            this.currentPage = curPage;
            LoadInfoOfPage(ref start, ref end);

            for (int i = start; i < end; i++)
            {
                EmployeeControl employeeControl = new EmployeeControl();
                EmployeePosition employeePosition = EmployeePositionDAL.Instance.GetById(employeeList[i].IdPosition);
                employeeControl.txbId.Text = AddPrefix("NV", employeeList[i].IdEmployee);
                employeeControl.txbName.Text = employeeList[i].Name.ToString();
                employeeControl.txbPosition.Text = employeePosition.Position;
                employeeControl.txbTotalSalary.Text = SeparateThousands(employeePosition.SalaryBase.ToString());
                employeeControl.txbPhoneNumber.Text = employeeList[i].PhoneNumber.ToString();
                employeeControl.txbAddress.Text = employeeList[i].Address.ToString();

                mainWindow.stkEmployeeList.Children.Add(employeeControl);
            }
        }
        public void LoadInfoOfPage(ref int start, ref int end)
        {
            mainWindow.btnPrePageEmp.IsEnabled = currentPage == 0 ? false : true;
            mainWindow.btnNextPageEmp.IsEnabled = currentPage == (employeeList.Count - 1) / 10 ? false : true;

            start = currentPage * 10;
            end = (currentPage + 1) * 10;
            if (currentPage == employeeList.Count / 10)
                end = employeeList.Count;

            mainWindow.txtNumOfEmp.Text = String.Format("Trang {0} trên {1} trang", currentPage + 1, employeeList.Count / 11 + 1);
        }
        public void GoToNextPage(MainWindow mainWindow, int currentPage)
        {
            LoadEmployeeList(mainWindow, currentPage);
        }
        public void GoToPreviousPage(MainWindow mainWindow, int currentPage)
        {
            LoadEmployeeList(mainWindow, currentPage);
        }
        public void Sort(MainWindow mainWindow)
        {
            switch (mainWindow.cboSortEmployee.SelectedIndex)
            {
                case -1:
                    return;
                case 0:
                    employeeList = employeeList.OrderBy(x => x.Name).ToList();
                    break;
                case 1:
                    employeeList = employeeList.OrderByDescending(x => x.Name).ToList();
                    break;
            }
            LoadEmployeeList(mainWindow, 0);
        }
        public void Filter(MainWindow mainWindow)
        {
            if (mainWindow.cboFilterPosition.SelectedIndex == -1)
                return;
            string nameSearching = mainWindow.txtSearchEmployee.Text.ToLower();
            employeeList = EmployeeDAL.Instance.FindByName(nameSearching);
            if (mainWindow.cboFilterPosition.SelectedIndex == 0)
            {
                LoadEmployeeList(mainWindow, 0);
                Sort(mainWindow);
                return;
            }
            employeeList.RemoveAll(x => x.IdPosition != FilterPosition.IdEmployeePosition);
            if (mainWindow.cboSortEmployee.SelectedIndex != -1)
            {
                Sort(mainWindow);
            }
            else
            {
                LoadEmployeeList(mainWindow, 0);
            }
        }
    }
}
