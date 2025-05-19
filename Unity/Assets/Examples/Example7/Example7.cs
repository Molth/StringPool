using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples
{
    public class Example7 : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        private Label healthLabel;
        private Label statusLabel;
        private Button attackButton;
        private int health = 100;
        private string _healthText;
        private UnsafeString _statusText;

        private void OnEnable()
        {
            _statusText ??= new UnsafeString();

            var root = uiDocument.rootVisualElement;
            healthLabel = root.Q<Label>("health-label");
            statusLabel = root.Q<Label>("status-label");
            attackButton = root.Q<Button>("attack-button");

            attackButton.clicked += OnAttackButtonClicked;

            UpdateHealthDisplay();
            UpdateStatusDisplay();
        }

        private void OnDisable()
        {
            _statusText?.Dispose();

            attackButton.clicked -= OnAttackButtonClicked;
        }

        private void OnAttackButtonClicked()
        {
            health -= 10;
            if (health < 0)
                health = 0;
            UpdateHealthDisplay();
            UpdateStatusDisplay();
            Debug.Log("Player attacked! Health: " + health);
        }

        private void UpdateHealthDisplay()
        {
            using var builder = ZString.CreateStringBuilder();
            builder.Append("Health: ");
            builder.Append(health);

            if (_healthText != null)
                StringPool.Shared.Return(_healthText);

            var text = StringPool.Shared.Rent(10000);
            var span = text.AsSpan();
            var span2 = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
            builder.TryCopyTo(span2, out _);

            _healthText = text;

            // TODO: must refresh reference
            healthLabel.text = "";

            healthLabel.text = text;
        }

        private void UpdateStatusDisplay()
        {
            using var builder = ZString.CreateStringBuilder();
            builder.Append("Status: ");

            if (health >= 100)
                builder.Append("Healthy");
            else if (health >= 50)
                builder.Append("Mid");
            else if (health >= 20)
                builder.Append("Low");
            else if (health <= 0)
                builder.Append("Dead");
            else
                builder.Append("Dying");

            _statusText.SetText(StringPool.Shared.Rent(builder.AsSpan()));

            // TODO: must refresh reference
            statusLabel.text = "";

            statusLabel.text = _statusText;
        }
    }
}