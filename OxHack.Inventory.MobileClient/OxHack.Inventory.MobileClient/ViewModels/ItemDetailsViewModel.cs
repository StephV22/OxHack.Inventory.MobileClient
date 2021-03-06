﻿using OxHack.Inventory.ApiClient;
using OxHack.Inventory.ApiClient.Models;
using OxHack.Inventory.MobileClient.Views;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.ComponentModel;

namespace OxHack.Inventory.MobileClient.ViewModels
{
	public class ItemDetailsViewModel : PageViewModelBase
	{
		private readonly InventoryClient inventoryClient;
		private bool isEditing;

		private Guid id;
		private string concurrencyId;
		private int version;
		private List<EditFieldViewModelBase> fields;

		public ItemDetailsViewModel(INavigation navigation, InventoryClient inventoryClient, Item model, bool isEditing = false)
		: base(navigation)
		{
			this.inventoryClient = inventoryClient;

			this.InitialiseEditFields();
			this.LoadModel(model);

			this.IsEditing = isEditing;
		}

		private void InitialiseEditFields()
		{
			this.Name = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.Manufacturer = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.Model = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.Quantity = new EditFieldViewModel<int>(async () => await this.SaveItemChangeAsync());
			this.Category = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.Spec = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.Appearance = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.CurrentLocation = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.AssignedLocation = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.IsLoan = new EditFieldViewModel<bool>(async () => await this.SaveItemChangeAsync());
			this.Origin = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());
			this.AdditionalInformation = new EditFieldViewModel<string>(async () => await this.SaveItemChangeAsync());

			var fields = new List<EditFieldViewModelBase>();

			fields.Add(this.Name);
			fields.Add(this.Manufacturer);
			fields.Add(this.Model);
			fields.Add(this.Quantity);
			fields.Add(this.Category);
			fields.Add(this.Spec);
			fields.Add(this.Appearance);
			fields.Add(this.CurrentLocation);
			fields.Add(this.AssignedLocation);
			fields.Add(this.IsLoan);
			fields.Add(this.Origin);
			fields.Add(this.AdditionalInformation);

			foreach (var field in fields)
			{
				field.PropertyChanged += this.OnFieldPropertyChanged;
			}

			this.fields = fields;
		}

		private void OnFieldPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(EditFieldViewModel<Object>.IsInEditMode))
			{
				var isAnyInEditMode = this.fields.Any(item => item.IsInEditMode);

				foreach (var field in this.fields)
				{
					field.IsEditEnabled = !isAnyInEditMode;
				}
			}
		}

		private void LoadModel(Item model)
		{
			this.id = model.Id;
			this.concurrencyId = model.ConcurrencyId;
			this.version = model.Version;

			this.Name.Value = model.Name;
			this.Manufacturer.Value = model.Manufacturer;
			this.Model.Value = model.Model;
			this.Quantity.Value = model.Quantity;
			this.Category.Value = model.Category;
			this.Spec.Value = model.Spec;
			this.Appearance.Value = model.Appearance;
			this.CurrentLocation.Value = model.CurrentLocation;
			this.AssignedLocation.Value = model.AssignedLocation;
			this.IsLoan.Value = model.IsLoan;
			this.Origin.Value = model.Origin;
			this.AdditionalInformation.Value = model.AdditionalInformation;

			this.Photos = model.Photos.ToList();
			this.OnPropertyChanged(nameof(this.Photos));
		}

		private Item CopyToModel()
		{
			var model = new Item(
				this.id,
				this.version,
				this.AdditionalInformation.Value,
				this.Appearance.Value,
				this.AssignedLocation.Value,
				this.Category.Value,
				this.CurrentLocation.Value,
				this.IsLoan.Value,
				this.Manufacturer.Value,
				this.Model.Value,
				this.Name.Value,
				this.Origin.Value,
				this.Quantity.Value,
				this.Spec.Value,
				this.Photos.ToList(),
				this.concurrencyId);

			return model;
		}

		private async Task SaveItemChangeAsync()
        {
            // Eventually, when OData is implemented, send over just the Delta using PATCH.
            // For now we send over the whole entity.
            await this.inventoryClient.SaveItemAsync(this.CopyToModel());
            await this.ReloadAfterSave();
        }

        private async Task SavePhotoAdditionAsync(Uri added)
        {
            await this.inventoryClient.AddPhotos(this.id, this.concurrencyId, added);
			await this.ReloadAfterSave();
        }

        private async Task SavePhotoRemovalAsync(Uri removed)
        {
            await this.inventoryClient.RemovePhotos(this.id, this.concurrencyId, removed);
            await this.ReloadAfterSave();
        }

        private async Task ReloadAfterSave()
        {
            // TODO: Start an animation here.
            var tryCount = 3;
            for (int i = 1; i <= tryCount; i++)
            {
                await Task.Delay(500 * i);
                var update = await this.inventoryClient.GetItemByIdAsync(this.id);

                if (update.Version > this.version)
                {
                    this.LoadModel(update);
                    break;
                }
                if (i == tryCount)
                {
                    // TODO: Show popup saying something to the effect of "Could not retrieve updated version of Item."
                    await this.Navigation.PopAsync();
                }
            }
            // TODO: End the animation here.
        }

        public string Title
		{
			get
			{
				return
					this.IsEditing
						? "Editing Item Details"
						: "Item Details";
			}
		}

		public bool IsEditing
		{
			get
			{
				return this.isEditing;
			}
			set
			{
				base.SetProperty(ref this.isEditing, value);
				base.OnPropertyChanged(nameof(this.IsNotEditing));
			}
		}

		public bool IsNotEditing
			=> !this.IsEditing;


		public EditFieldViewModel<string> Name
		{
			get; set;
		}

		public EditFieldViewModel<string> Manufacturer
		{
			get; set;
		}

		public EditFieldViewModel<string> Model
		{
			get; set;
		}

		public EditFieldViewModel<int> Quantity
		{
			get; set;
		}

		public EditFieldViewModel<string> Category
		{
			get; set;
		}

		public EditFieldViewModel<string> Spec
		{
			get; set;
		}

		public EditFieldViewModel<string> Appearance
		{
			get; set;
		}

		public EditFieldViewModel<string> AssignedLocation
		{
			get; set;
		}

		public EditFieldViewModel<string> CurrentLocation
		{
			get; set;
		}

		public EditFieldViewModel<bool> IsLoan
		{
			get; set;
		}

		public EditFieldViewModel<string> Origin
		{
			get; set;
		}

		public EditFieldViewModel<string> AdditionalInformation
		{
			get; set;
		}

		public List<Uri> Photos
		{
			get; set;
		}
		
		public DelegateCommand<Uri> AddPhotoCommand
			=> new DelegateCommand<Uri>(async photo => await this.SavePhotoAdditionAsync(photo));

		public DelegateCommand<Uri> RemovePhotoCommand
			=> new DelegateCommand<Uri>(async photo => await this.SavePhotoRemovalAsync(photo));
    }
}