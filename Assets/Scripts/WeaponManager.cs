using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Настройки выбора")]
    public bool randomSelection = true; // Если false, выберется оружие по индексу ниже
    public int defaultWeaponIndex = 0;  // Индекс оружия при ручном выборе

    [Header("Список префабов оружия")]
    [SerializeField] private List<GameObject> weaponPrefabs; // Перетащи сюда Katana и Gun

    [Header("Точка спавна (левая рука)")]
    [SerializeField] private Transform leftHandSlot;

    private GameObject currentWeaponInstance;

    void Start()
    {
        SpawnSelectedWeapon();
    }

    void SpawnSelectedWeapon()
    {
        if (weaponPrefabs == null || weaponPrefabs.Count == 0)
        {
            Debug.LogError("[WeaponManager] Список оружия пуст!");
            return;
        }

        if (leftHandSlot == null)
        {
            Debug.LogError("[WeaponManager] Слот левой руки не назначен!");
            return;
        }

        // Удаляем старое оружие, если оно есть
        if (currentWeaponInstance != null)
            Destroy(currentWeaponInstance);

        // Логика выбора
        int index = randomSelection ? Random.Range(0, weaponPrefabs.Count) : defaultWeaponIndex;

        // Защита от выхода за границы массива
        index = Mathf.Clamp(index, 0, weaponPrefabs.Count - 1);

        // Спавн
        currentWeaponInstance = Instantiate(weaponPrefabs[index], leftHandSlot.position, Quaternion.identity, leftHandSlot);

        // Важно: сбрасываем локальную позицию/ротацию, чтобы оружие встало ровно в слот
        currentWeaponInstance.transform.localPosition = Vector3.zero;
        currentWeaponInstance.transform.localRotation = Quaternion.identity;

        Debug.Log($"[WeaponManager] Выбрано оружие: {weaponPrefabs[index].name}");
    }

    // Можно вызывать из UI или других скриптов для смены оружия во время игры
    public void ChangeWeapon(int index)
    {
        randomSelection = false;
        defaultWeaponIndex = index;
        SpawnSelectedWeapon();
    }
}
