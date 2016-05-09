﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxHack.Inventory.MobileClient.ViewModels;
using Xamarin.Forms;
using OxHack.Inventory.WebClient.Models;

namespace OxHack.Inventory.MobileClient.Views
{
    public partial class CategoriesPage : ContentPage
    {
        public CategoriesPage(CategoriesViewModel viewModel)
        {
            InitializeComponent();

            this.BindingContext = viewModel;
        }
    }
}
