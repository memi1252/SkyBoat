using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using RenderSettings = UnityEngine.RenderSettings;

[System.Serializable]
public class WeightedSpawnItem
{
    public GameObject prefab;

    [Range(0.01f, 100f)] // 가중치는 0보다 큰 값으로 설정하도록 제한
    public float weight = 1f;
}

public enum IslandType
{
    resource,
    battle,
    Event,
    Shop
}

[System.Serializable]
public struct Island
{
    public GameObject[] BaseBuilds;
    public WeightedSpawnItem[] spawnItems;
    public WeightedSpawnItem[] spawnProps;
    public int numberOfObjects;
    public int numberOfItem;
    public Material skyboxMaterial;
    public Material islandMaterial;
}

public class RandomObjectSpawn : MonoBehaviour
{
    public Island[] islands;
    public IslandType islandType;
    public Renderer islandRenderer;
    public Light light;
    public GameObject monsterPrefab;
    public int numberOfMonsters;
    

    // 레이캐스트가 시작될 높이 (타겟 오브젝트보다 충분히 높게 설정)
    private const float RAYCAST_START_HEIGHT = 100f;

    void Start()
    {
        int index = 0;
        switch (islandType)
        {
            case IslandType.resource:
                index = 0;
                light.intensity = 2;
                break;
            case IslandType.battle:
                light.intensity = 0.05f;
                index = 1;
                break;
            case IslandType.Event:
                index = 2;
                break;
        }

        islandRenderer.material = islands[index].islandMaterial;
        RenderSettings.skybox = islands[index].skyboxMaterial;
        var baseBuild = Instantiate(islands[index].BaseBuilds[0],
            transform.position + new Vector3(0, 100, 0), Quaternion.identity);
        baseBuild.transform.position =
            new Vector3(transform.position.x, GetSpawnHeight(baseBuild), transform.position.z);
        baseBuild.transform.SetParent(transform);
        if (baseBuild.GetComponentInChildren<MeshCollider>())
        {
            baseBuild.GetComponentInChildren<MeshCollider>().enabled = true;
        }
        SpawnRandomObjectsOnTopAccurate(index);
        SpawnRandomItemsOnTopAccurate(index);
        if (islandType == IslandType.battle)
        {
            SpawnRandomMonsterOnTopAccurate();
        }
    }

    public void IslandChange(int indee)
    {
        Destroy(transform.GetChild(0).gameObject);
        for (int i = transform.childCount-1; i > 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        StartCoroutine(Dd(indee));
    }

    IEnumerator Dd(int indee)
    {
        yield return new WaitForSecondsRealtime(1);
        int index = 0;
        switch (indee)
        {
            case 0:
                index = 0;
                light.intensity = 2;
                break;
            case 1:
                light.intensity = 0.05f;
                index = 1;
                break;
            case 2:
                index = 2;
                break;
            case 3:
                LoadingBar.LoadScene("engging");
                break;
        }

        islandRenderer.material = islands[index].islandMaterial;
        RenderSettings.skybox = islands[index].skyboxMaterial;
        
        var baseBuild = Instantiate(islands[index].BaseBuilds[0],
            transform.position + new Vector3(0, 100, 0), Quaternion.identity);
        baseBuild.transform.position =
            new Vector3(transform.position.x, GetSpawnHeight(baseBuild), transform.position.z);
        baseBuild.transform.SetParent(transform);
        if (baseBuild.GetComponentInChildren<MeshCollider>())
        {
            baseBuild.GetComponentInChildren<MeshCollider>().enabled = true;
        }
        SpawnRandomObjectsOnTopAccurate(index);
        SpawnRandomItemsOnTopAccurate(index);
        if (indee == 1)
        {
            SpawnRandomMonsterOnTopAccurate();
        }
    }

    // 가중치에 따라 생성할 오브젝트 프리팹을 선택하는 함수 (이전과 동일)
    private GameObject GetRandomWeightedObject(int index)
    {
        if (islands[index].spawnProps == null || islands[index].spawnProps.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var item in islands[index].spawnProps)
        {
            if (item.weight > 0) totalWeight += item.weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        foreach (var item in islands[index].spawnProps)
        {
            if (item.weight > 0)
            {
                currentWeight += item.weight;
                if (randomValue <= currentWeight) return item.prefab;
            }
        }

        return islands[index].spawnProps[0].prefab;
    }

    private GameObject GetRandomWeightedItem(int index)
    {
        if (islands[index].spawnItems == null || islands[index].spawnItems.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var item in islands[index].spawnItems)
        {
            if (item.weight > 0) totalWeight += item.weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        foreach (var item in islands[index].spawnItems)
        {
            if (item.weight > 0)
            {
                currentWeight += item.weight;
                if (randomValue <= currentWeight) return item.prefab;
            }
        }

        return islands[index].spawnItems[0].prefab;
    }

    // 생성된 인스턴스의 콜라이더를 기반으로 표면에 닿도록 필요한 높이(절반 크기)를 계산합니다.
    private float GetSpawnHeight(GameObject spawnedObject)
    {
        if (islandType == IslandType.Event)
        {
            if (spawnedObject.CompareTag("MainBuild"))
            {
                if (Physics.Raycast(spawnedObject.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    spawnedObject.transform.SetParent(transform);
                    return hit.point.y;
                }
            }else
            {
                if (Physics.Raycast(spawnedObject.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    return hit.point.y + Random.Range(0, 15f);
                }
            }
        }
        else
        {
            if (Physics.Raycast(spawnedObject.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                return hit.point.y;
            }
        }
        

        return 0;
        // Collider collider = spawnedObject.GetComponent<Collider>();
        // if (collider != null)
        // {
        //     // 콜라이더의 바운딩 박스 크기(size.y)의 절반을 사용합니다. 
        //     // 이는 오브젝트의 중심(pivot)이 표면에 오게 한 후, 절반만큼 올려야 바닥에 닿기 때문입니다.
        //     return collider.bounds.extents.y;
        // }
        //
        // // 콜라이더가 없다면 기본값 사용 (대개 유니티 기본 큐브의 절반 높이)
        // return 0.5f; 
    }

    // 레이캐스트를 사용하여 타겟 오브젝트의 표면 위에 정확히 생성하는 함수
    void SpawnRandomObjectsOnTopAccurate(int index)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer (Mesh Renderer 등) 또는 Collider 컴포넌트가 오브젝트에 없습니다.");
            return;
        }

        Bounds bounds = renderer.bounds;
        int objectsSpawned = 0;

        int maxAttempts = islands[index].numberOfObjects * 20;
        while (objectsSpawned < islands[index].numberOfObjects && maxAttempts > 0)
        {
            maxAttempts--;

            GameObject selectedPrefab = GetRandomWeightedObject(index);
            if (selectedPrefab == null) continue;

            // 1. **임시 위치에 오브젝트를 먼저 생성**합니다. (높이 계산을 위해)
            // 레이캐스트 시작 위치 근처에 생성합니다. 중요한 것은 **스케일 적용된 콜라이더**를 얻는 것입니다.
            GameObject instance = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
            // 2. 바운딩 박스 내 랜덤 위치를 계산하고 레이캐스트를 쏩니다.
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 rayStartPoint = new Vector3(randomX, bounds.max.y + RAYCAST_START_HEIGHT, randomZ);

            RaycastHit hit;
            if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // 3. **생성된 인스턴스**의 실제 높이 절반을 계산합니다.
                    float currentSpawnObjectHalfHeight = GetSpawnHeight(instance);

                    // 4. 오브젝트가 표면 위에 놓일 최종 위치를 계산합니다.
                    Vector3 spawnPosition = hit.point + Vector3.up * currentSpawnObjectHalfHeight;
                    Debug.Log(spawnPosition);
                    // 5. 인스턴스의 위치를 최종 위치로 설정합니다.
                    instance.transform.position = spawnPosition;
                    if (islandType == IslandType.Event)
                    {
                        instance.transform.rotation = Random.rotation;
                    }
                    instance.transform.SetParent(transform);
                    objectsSpawned++;
                }
                else
                {
                    Debug.Log("!");
                    Destroy(instance);
                }
            }
            else
            {
                // 레이캐스트가 아무것도 맞추지 못한 경우, 임시로 생성했던 인스턴스를 제거합니다.
                Debug.Log("!");
                Destroy(instance);
            }
        }
    }

    void SpawnRandomItemsOnTopAccurate(int index)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer (Mesh Renderer 등) 또는 Collider 컴포넌트가 오브젝트에 없습니다.");
            return;
        }

        Bounds bounds = renderer.bounds;
        int objectsSpawned = 0;
        int maxAttempts = islands[index].numberOfItem * 20;

        while (objectsSpawned < islands[index].numberOfItem && maxAttempts > 0)
        {
            maxAttempts--;

            GameObject selectedPrefab = GetRandomWeightedItem(index);
            if (selectedPrefab == null) continue;
            selectedPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            // 1. **임시 위치에 오브젝트를 먼저 생성**합니다. (높이 계산을 위해)
            // 레이캐스트 시작 위치 근처에 생성합니다. 중요한 것은 **스케일 적용된 콜라이더**를 얻는 것입니다.
            GameObject instance = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(transform);
            // 2. 바운딩 박스 내 랜덤 위치를 계산하고 레이캐스트를 쏩니다.
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 rayStartPoint = new Vector3(randomX, bounds.max.y + RAYCAST_START_HEIGHT, randomZ);

            RaycastHit hit;

            if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // 3. **생성된 인스턴스**의 실제 높이 절반을 계산합니다.
                    float currentSpawnObjectHalfHeight = GetSpawnHeight(instance);

                    // 4. 오브젝트가 표면 위에 놓일 최종 위치를 계산합니다.
                    Vector3 spawnPosition = hit.point + Vector3.up * currentSpawnObjectHalfHeight;
                    Debug.Log(spawnPosition);
                    // 5. 인스턴스의 위치를 최종 위치로 설정합니다.
                    instance.transform.position = spawnPosition;

                    objectsSpawned++;
                }
                else
                {
                    // 레이캐스트가 다른 오브젝트에 맞은 경우, 임시로 생성했던 인스턴스를 제거합니다.
                    Destroy(instance);
                }
            }
            else
            {
                // 레이캐스트가 아무것도 맞추지 못한 경우, 임시로 생성했던 인스턴스를 제거합니다.
                Destroy(instance);
            }
        }
    }
    
    void SpawnRandomMonsterOnTopAccurate()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer (Mesh Renderer 등) 또는 Collider 컴포넌트가 오브젝트에 없습니다.");
            return;
        }

        Bounds bounds = renderer.bounds;
        int objectsSpawned = 0;
        int maxAttempts = numberOfMonsters * 20;

        while (objectsSpawned < numberOfMonsters  && maxAttempts > 0)
        {
            maxAttempts--;

            GameObject selectedPrefab = monsterPrefab;
            if (selectedPrefab == null) continue;
            selectedPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            // 1. **임시 위치에 오브젝트를 먼저 생성**합니다. (높이 계산을 위해)
            // 레이캐스트 시작 위치 근처에 생성합니다. 중요한 것은 **스케일 적용된 콜라이더**를 얻는 것입니다.
            GameObject instance = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(transform);
            // 2. 바운딩 박스 내 랜덤 위치를 계산하고 레이캐스트를 쏩니다.
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 rayStartPoint = new Vector3(randomX, bounds.max.y + RAYCAST_START_HEIGHT, randomZ);

            RaycastHit hit;

            if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // 3. **생성된 인스턴스**의 실제 높이 절반을 계산합니다.
                    float currentSpawnObjectHalfHeight = GetSpawnHeight(instance);

                    // 4. 오브젝트가 표면 위에 놓일 최종 위치를 계산합니다.
                    Vector3 spawnPosition = hit.point + Vector3.up * currentSpawnObjectHalfHeight;
                    Debug.Log(spawnPosition);
                    // 5. 인스턴스의 위치를 최종 위치로 설정합니다.
                    instance.transform.position = spawnPosition;

                    objectsSpawned++;
                }
                else
                {
                    // 레이캐스트가 다른 오브젝트에 맞은 경우, 임시로 생성했던 인스턴스를 제거합니다.
                    Destroy(instance);
                }
            }
            else
            {
                // 레이캐스트가 아무것도 맞추지 못한 경우, 임시로 생성했던 인스턴스를 제거합니다.
                Destroy(instance);
            }
        }
    }
    
}