using System;
using System.Collections.Generic;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.UI.Screens
{
    public class InventoryScreen : View
    {
        private readonly GameState _gameState;
        private ListView _inventoryListView;
        private Label _itemDetailsLabel;
        private Button _useButton;
        private Button _dropButton;
        private Button _closeButton;
        
        private List<Item> _displayedItems;
        private Item? _selectedItem;
        
        public InventoryScreen(GameState gameState)
        {
            _gameState = gameState;
            _displayedItems = new List<Item>();
            
            // Set up to fill its container
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            
            // Create list view for inventory items
            _inventoryListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(40),
                Height = Dim.Fill() - 2, // Leave space for buttons at bottom
                AllowsMarking = false,
                ColorScheme = new ColorScheme()
                {
                    Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                    Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
                    HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                    HotFocus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Gray)
                }
            };
            
            // Create item details view
            _itemDetailsLabel = new Label()
            {
                X = Pos.Right(_inventoryListView) + 2,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2 // Leave space for buttons at bottom
            };
            
            // Create action buttons
            _useButton = new Button("Use/Equip")
            {
                X = 0,
                Y = Pos.AnchorEnd(1),
                Enabled = false
            };
            
            _dropButton = new Button("Drop")
            {
                X = Pos.Right(_useButton) + 2,
                Y = Pos.AnchorEnd(1),
                Enabled = false
            };
            
            _closeButton = new Button("Close")
            {
                X = Pos.Right(_dropButton) + 2,
                Y = Pos.AnchorEnd(1)
            };
            
            // Add event handlers
            _inventoryListView.SelectedItemChanged += OnInventorySelectionChanged;
            _useButton.Clicked += OnUseButtonClicked;
            _dropButton.Clicked += OnDropButtonClicked;
            _closeButton.Clicked += OnCloseButtonClicked;
            
            // Add UI elements to the view
            Add(_inventoryListView);
            Add(_itemDetailsLabel);
            Add(_useButton);
            Add(_dropButton);
            Add(_closeButton);
        }
        
        public void RefreshInventory()
        {
            // Get items from player's inventory
            _displayedItems = _gameState.Player.Inventory;
            
            // Create list of item names to display
            List<string> itemNames = new List<string>();
            foreach (var item in _displayedItems)
            {
                string equipIndicator = item.IsEquipped ? " [E]" : "";
                string itemName = $"{item.Name}{equipIndicator}";
                itemNames.Add(itemName);
            }
            
            // Update list view
            _inventoryListView.SetSource(itemNames);
            
            // Clear selection if inventory is empty
            if (_displayedItems.Count == 0)
            {
                _selectedItem = null;
                _itemDetailsLabel.Text = "No items in inventory.";
                _useButton.Enabled = false;
                _dropButton.Enabled = false;
            }
            else if (_selectedItem == null)
            {
                // Select first item if nothing is selected
                _inventoryListView.SelectedItem = 0;
            }
        }
        
        private void OnInventorySelectionChanged(ListViewItemEventArgs e)
        {
            if (_displayedItems.Count == 0 || e.Item < 0 || e.Item >= _displayedItems.Count)
            {
                _selectedItem = null;
                _itemDetailsLabel.Text = "";
                _useButton.Enabled = false;
                _dropButton.Enabled = false;
                return;
            }
            
            // Update selected item
            _selectedItem = _displayedItems[e.Item];
            
            // Update item details view
            if (_selectedItem != null)
            {
                _itemDetailsLabel.Text = _selectedItem.GetDescription();
                
                // Update button labels and enabled state based on item type
                if (_selectedItem.Type == ItemType.Consumable)
                {
                    _useButton.Text = "Use";
                    _useButton.Enabled = true;
                }
                else
                {
                    _useButton.Text = _selectedItem.IsEquipped ? "Unequip" : "Equip";
                    _useButton.Enabled = true;
                }
                
                _dropButton.Enabled = !_selectedItem.IsEquipped; // Can't drop equipped items
            }
            else
            {
                _itemDetailsLabel.Text = "";
                _useButton.Enabled = false;
                _dropButton.Enabled = false;
            }
        }
        
        private void OnUseButtonClicked()
        {
            if (_selectedItem == null)
                return;
            
            if (_selectedItem.Type == ItemType.Consumable)
            {
                // Use consumable item
                _gameState.Player.EquipItem(_selectedItem); // This will use the consumable
                RefreshInventory(); // Refresh to remove the used item
            }
            else if (_selectedItem.IsEquipped)
            {
                // Unequip item
                _gameState.Player.UnequipItem(_selectedItem);
                RefreshInventory();
            }
            else
            {
                // Equip item
                _gameState.Player.EquipItem(_selectedItem);
                RefreshInventory();
            }
        }
        
        private void OnDropButtonClicked()
        {
            if (_selectedItem == null || _selectedItem.IsEquipped)
                return;
            
            // Drop item at player's position
            _gameState.UpdateItemPosition(_selectedItem, _gameState.Player.Position.X, _gameState.Player.Position.Y);
            _gameState.AddDroppedItem(_selectedItem);
            _gameState.Player.RemoveItemFromInventory(_selectedItem);
            
            // Refresh the inventory display
            RefreshInventory();
        }
        
        private void OnCloseButtonClicked()
        {
            // Explicitly request to stop the application modal
            Application.RequestStop();
        }
        
        public void LoadInventory()
        {
            RefreshInventory();
        }
    }
}