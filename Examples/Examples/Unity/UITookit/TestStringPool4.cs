#if UNITY_2021_3_OR_NEWER
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples
{
    public class TestStringPool4 : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        private Label healthLabel;
        private Label statusLabel;
        private Button attackButton;
        private int health = 100;
        private string _healthText;
        private string _statusText;

        private void OnEnable()
        {
            VisualElement root = uiDocument.rootVisualElement;
            healthLabel = root.Q<Label>("health-label");
            statusLabel = root.Q<Label>("status-label");
            attackButton = root.Q<Button>("attack-button");

            attackButton.clicked += OnAttackButtonClicked;

            UpdateHealthDisplay();
            UpdateStatusDisplay();
        }

        private void OnDisable()
        {
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
            using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
            builder.Append("Health: ");
            builder.Append(health);

            if (_healthText != null)
                StringPool.Shared.Return(_healthText);

            string text = StringPool.Shared.Rent(10000);
            ReadOnlySpan<char> span = text.AsSpan();
            Span<char> span2 = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
            builder.TryCopyTo(span2, out _);

            _healthText = text;

            // must refresh reference
            healthLabel.text = "";

            healthLabel.text = text;
        }

        private void UpdateStatusDisplay()
        {
            using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
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

            if (_statusText != null)
                StringPool.Shared.Return(_statusText);

            string text = StringPool.Shared.Rent(builder.AsSpan());

            _statusText = text;

            // must refresh reference
            statusLabel.text = "";

            statusLabel.text = text;
        }
    }
}
#endif