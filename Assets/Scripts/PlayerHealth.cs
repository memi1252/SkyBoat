using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("UI Elements")]
    public Slider healthBar;
    public Text healthText;
    
    [Header("Death Settings")]
    public GameObject deathScreen;
    
    private bool isDead = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateHealthUI();
    }
    
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        // 죽음 화면 활성화
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
        
        // 플레이어 컨트롤 비활성화
        var playerController = GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        Debug.Log("Player died!");
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();
        
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }
        
        // 플레이어 컨트롤 다시 활성화
        var playerController = GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}
