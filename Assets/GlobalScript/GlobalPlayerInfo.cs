using UnityEngine;

public class GlobalPlayerInfo : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = 5f;
    public float attackCooldown = 0.5f;
    public GameObject bulletPrefab;
    public float rotationSpeed = 720f;
    public float rotationOffset = 0f;
    public float bulletSpeed = 10f;
    public GameObject BloodBag1;
    public GameObject BloodBag2;
    public GameObject BloodBag3;
    public GameObject Store;
    public GameObject[] BloodBagOptions;
    public Money Money;
    public float levelTax = 0.05f;
    public float bankRate = 0.03f;
    public GameObject BankBag1;
    public GameObject BankBag2;
    public GameObject BankBag3;

    private void Awake()
    {
        mainCamera = Camera.main;
    }
}
