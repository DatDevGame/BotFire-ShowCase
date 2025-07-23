using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class HexWaveGenerator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The size (radius) of a single hex tile.")]
    public float hexSize = 1f;
    [Tooltip("The prefab to instantiate for each hex in the wave.")]
    public GameObject hexPrefab;
    [Header("Wave Parameters")]
    [Tooltip("The center transform from which the wave originates.")]
    public Transform centerObject;
    [Min(0)]
    [Tooltip("The number of levels (distance) out from the center.")]
    public int waveLevel = 1;

    private List<GameObject> currentWaveHexes = new List<GameObject>();

    /// <summary>
    /// Generates a hexagonal wave (ring) at a specific level 'n' around a world position.
    /// </summary>
    /// <param name="worldCenter">The center point in world space.</param>
    /// <param name="level">The distance (level) of the ring from the center.</param>
    public void GenerateWave(Vector3 worldCenter, int level)
    {
        if (hexPrefab == null)
        {
            Debug.LogError("Hex Prefab is not assigned!", this);
            return;
        }
        if (hexSize <= 0f)
        {
            Debug.LogError("Hex Size must be positive!", this);
            return;
        }
        if (level < 0)
        {
            Debug.LogWarning("Wave level 'n' cannot be negative. Setting to 0.", this);
            level = 0;
        }


        // Clear any previously generated wave
        //ClearWave();

        // 1. Convert world center to hex coordinates
        HexCoords centerCoords = HexCoords.FromWorldPosition(worldCenter, hexSize);

        // 2. Get the coordinates of all hexes in the ring at level 'n'
        List<HexCoords> ringCoords = GetHexRing(centerCoords, level);

        // 3. Instantiate prefabs at the world positions of the ring hexes
        foreach (HexCoords coord in ringCoords)
        {
            Vector3 worldPos = coord.ToWorldPosition(hexSize);
            GameObject hexInstance = Instantiate(hexPrefab, worldPos, Quaternion.identity, this.transform); // Parent to generator
            currentWaveHexes.Add(hexInstance);
            // Optional: Customize the instantiated hex (e.g., change color, add component)
            // hexInstance.GetComponent<Renderer>().material.color = Color.yellow;
        }

        Debug.Log($"Generated wave level {level} around {worldCenter} ({centerCoords}) with {ringCoords.Count} hexes.");
    }

    /// <summary>
    /// Calculates the HexCoords for all hexes exactly 'n' steps away from the center.
    /// </summary>
    /// <param name="center">The center hex coordinate.</param>
    /// <param name="level">The radius (distance) of the ring.</param>
    /// <returns>A List of HexCoords forming the ring.</returns>
    public List<HexCoords> GetHexRing(HexCoords center, int level)
    {
        List<HexCoords> results = new List<HexCoords>();

        if (level == 0)
        {
            // Ring at distance 0 is just the center hex itself
            results.Add(center);
            return results;
        }
        if (level < 0)
        {
            return results; // No hexes at negative distance
        }


        // Start at a hex 'n' steps away from the center along one axis.
        // Let's move along the +Q/-S direction (Direction 0)
        HexCoords current = center + HexCoords.Direction(0) * level;

        // Walk around the ring. There are 6 sides, each 'n' steps long.
        for (int i = 0; i < 6; i++) // Iterate through the 6 sides/directions
        {
            // The direction to step along this side is 'i + 2' % 6 relative to the starting direction 0.
            // E.g., start at (+n, 0, -n). First side moves along direction 2 (0, -1, 1).
            // Second side moves along direction 3 (-1, 0, 1), etc.
            // More simply: use directions 2, 3, 4, 5, 0, 1. Let's map i to direction index directly.
            int walkDirectionIndex = (i + 2) % 6; // Directions: 2, 3, 4, 5, 0, 1

            for (int j = 0; j < level; j++) // Take 'n' steps along this side
            {
                results.Add(current); // Add the current hex before moving
                current = current.Neighbor(walkDirectionIndex); // Move one step in the current side's direction
            }
        }
        // Note: The above loop adds the starting hex 'n' times. Using a HashSet automatically handles duplicates,
        // but the logic ensures each hex on the ring is visited exactly once IF the starting point calculation
        // and walk directions are correct. List is fine here.

        return results;
    }

    /// <summary>
    /// Destroys all GameObjects created for the current wave.
    /// </summary>
    [Button]
    public void ClearWave()
    {
        foreach (GameObject hex in currentWaveHexes)
        {
            if (hex != null) // Check if it hasn't been destroyed already
            {
                if(!Application.isPlaying)
                    DestroyImmediate(hex);
                else
                    Destroy(hex);
            }
        }
        currentWaveHexes.Clear();
    }
}

[Serializable]
public struct HexCoords
{
    // Cube coordinates (immutable)
    public readonly int Q; // Corresponds to Cube X
    public readonly int R; // Corresponds to Cube Z
    public readonly int S; // Corresponds to Cube Y

    // --- Static Directions ---
    // Define the 6 possible directions in cube coordinates
    private static readonly HexCoords[] directions = new HexCoords[] {
        new HexCoords(1, 0, -1), new HexCoords(1, -1, 0), new HexCoords(0, -1, 1),
        new HexCoords(-1, 0, 1), new HexCoords(-1, 1, 0), new HexCoords(0, 1, -1)
    };

    // --- Constructor ---
    public HexCoords(int q, int r, int s)
    {
        // Enforce the constraint x + y + z = 0
        if (q + r + s != 0)
        {
            Debug.LogError($"Invalid cube coordinates: {q}, {r}, {s}. Sum must be 0.");
            // Optionally throw an exception or default to (0,0,0)
            q = r = s = 0;
            // throw new ArgumentException("Cube coordinates must sum to 0.");
        }
        Q = q;
        R = r;
        S = s;
    }

    // Convenience constructor using only Q and R (most common)
    public HexCoords(int q, int r) : this(q, r, -q - r) { }

    // --- Static Methods ---

    // Get the direction vector for a given index (0 to 5)
    public static HexCoords Direction(int direction)
    {
        if (direction < 0 || direction >= directions.Length)
        {
            Debug.LogError("Invalid direction index: " + direction);
            return zero;
        }
        return directions[direction];
    }

    // --- Operators ---
    public static HexCoords operator +(HexCoords a, HexCoords b)
    {
        return new HexCoords(a.Q + b.Q, a.R + b.R, a.S + b.S);
    }

    public static HexCoords operator -(HexCoords a, HexCoords b)
    {
        return new HexCoords(a.Q - b.Q, a.R - b.R, a.S - b.S);
    }

    public static HexCoords operator *(HexCoords a, int k)
    {
        return new HexCoords(a.Q * k, a.R * k, a.S * k);
    }

    // --- Instance Methods ---

    // Get the neighbor in a specific direction
    public HexCoords Neighbor(int direction)
    {
        return this + Direction(direction);
    }

    // Calculate distance to another hex
    public int DistanceTo(HexCoords other)
    {
        // In cube coordinates, distance is half the Manhattan distance
        return (Mathf.Abs(Q - other.Q) + Mathf.Abs(R - other.R) + Mathf.Abs(S - other.S)) / 2;
    }

    // --- Equality & Hashing (Important for Collections like HashSet/Dictionary) ---
    public override bool Equals(object obj)
    {
        return obj is HexCoords other && Equals(other);
    }

    public bool Equals(HexCoords other)
    {
        return Q == other.Q && R == other.R && S == other.S;
    }

    public override int GetHashCode()
    {
        // Simple hash combining coordinates
        return HashCode.Combine(Q, R, S);
    }

    public static bool operator ==(HexCoords left, HexCoords right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HexCoords left, HexCoords right)
    {
        return !(left == right);
    }

    // --- String Representation ---
    public override string ToString()
    {
        return $"({Q}, {R}, {S})";
    }

    // --- Static Zero Coordinate ---
    public static readonly HexCoords zero = new HexCoords(0, 0, 0);


    // --- Coordinate Conversions ---

    // Convert World Position (Vector3 on XZ plane) to HexCoords
    // Assumes FLAT TOP orientation and hex origin at (0,0,0)
    public static HexCoords FromWorldPosition(Vector3 position, float hexSize)
    {
        if (hexSize <= 0)
        {
            Debug.LogError("Hex size must be positive.");
            return zero;
        }

        // Convert world coords to fractional axial coords (q, r)
        float q_frac = (Mathf.Sqrt(3f) / 3f * position.x - 1f / 3f * position.z) / hexSize;
        float r_frac = (2f / 3f * position.z) / hexSize;

        // Convert fractional axial to fractional cube coords (x, y, z)
        float x_frac = q_frac;
        float z_frac = r_frac;
        float y_frac = -x_frac - z_frac;

        // Round fractional cube coords to integer cube coords
        int q_int = Mathf.RoundToInt(x_frac);
        int r_int = Mathf.RoundToInt(z_frac);
        int s_int = Mathf.RoundToInt(y_frac);

        // Correct rounding errors to ensure q + r + s = 0
        float q_diff = Mathf.Abs(q_int - x_frac);
        float r_diff = Mathf.Abs(r_int - z_frac);
        float s_diff = Mathf.Abs(s_int - y_frac);

        if (q_diff > r_diff && q_diff > s_diff)
        {
            q_int = -r_int - s_int; // Recalculate q based on the other two
        }
        else if (r_diff > s_diff) // No need to check r_diff > q_diff again
        {
            r_int = -q_int - s_int; // Recalculate r
        }
        else
        {
            s_int = -q_int - r_int; // Recalculate s
        }

        return new HexCoords(q_int, r_int, s_int);
    }

    // Convert HexCoords to World Position (Vector3 on XZ plane) Center
    // Assumes FLAT TOP orientation and hex origin at (0,0,0)
    public Vector3 ToWorldPosition(float hexSize)
    {
        if (hexSize <= 0)
        {
            Debug.LogError("Hex size must be positive.");
            return Vector3.zero;
        }
        // Flat top orientation:
        float x = hexSize * (Mathf.Sqrt(3f) * Q + Mathf.Sqrt(3f) / 2f * R);
        float z = hexSize * (3f / 2f * R);
        return new Vector3(x, 0, z); // Use X and Z for world position
    }
}