using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @author Ciphered <https://ciphered.xyz
 * 
 * Generates a tree
 * Article link
 **/
public class Generator : MonoBehaviour {

	/**
	 * Represents a branch 
	 **/
	public class Branch {
		public Vector3 _start;
		public Vector3 _end;
		public Vector3 _direction;
		public Branch _parent;
		public float _size;
		public float _lastSize;
		public List<Branch> _children = new List<Branch>();
		public List<Vector3> _attractors = new List<Vector3>();
		public int _verticesId; // the index of the vertices within the vertices array 
		public int _distanceFromRoot = 0;
		public bool _grown = false;

		public Branch(Vector3 start, Vector3 end, Vector3 direction, Branch parent = null) {
			_start = start;
			_end = end;
			_direction = direction;
			_parent = parent;
		}
	}


	[Header("Generation parameters")]
	[Range(0, 3000)]
	public int _nbAttractors = 400;
	private int cnbAttr;
	[Range(0f, 100f)]
	public float _radius = 5f;
    [Range(0f, 100f)]
    public float _addRadius = 5f;
    //[SerializeField]
    //private Transform entPos;
    public Vector3 _startPosition { get { return transform.position; } }//entPos.position; } } // = new Vector3(0, 0, 0);
	[Range(0f, 1f)]
	public float _branchLength = 0.2f;
	[Range(0f, 1f)]
	public float _timeBetweenIterations = 0.5f;
	[Range(0f, 3f)]
	public float _attractionRange = 0.1f;
	[Range(0f, 2f)]
	public float _killRange = 0.5f;
	[Range(0f, 0.2f)]
	public float _randomGrowth = 0.1f;

	[Header("Mesh generation")]
	[Range(0, 20)]
	public int _radialSubdivisions = 10;
	[Range(0f, 1f), Tooltip("The size at the extremity of the branches")]
	public float _extremitiesSize = 0.05f;
	[Range(0f, 5f), Tooltip("Growth power, of the branches size")]
	public float _invertGrowth = 2f;


	// the attractor points
	public List<Vector3> _attractors = new List<Vector3>();

	// a list of the active attractors 
	public List<int> _activeAttractors = new List<int>();

	// reference to the first branch 
	Branch _firstBranch;

	// the branches 
	List<Branch> _branches = new List<Branch>();

	// a list of the current extremities 
	public List<Branch> _extremities = new List<Branch>();

	// the elpsed time since the last iteration, this is used for the purpose of animation
	float _timeSinceLastIteration = 0f;

	MeshFilter _filter;


	public bool IsGrowing = false;
	private float prevRad = 0f;
	[SerializeField]
	private bool extendedGrowth;
	[SerializeField]
	private int cutoff = 5;
	[SerializeField]
	private bool isRoot;
    [SerializeField][Range(0f, 5000f)]
    private float _childChance = 5f;
	[SerializeField]
	private List<Generator> childTrees = new List<Generator>();
	[SerializeField]
	private GameObject childPrefab;
	[SerializeField]
	private Vector3 crownOffset;
	[SerializeField]
	private int nbRingTrees;

    void Awake () {
		// initilization 
	}

	/**
	 * Generates n attractors and stores them in the attractors array
	 * The points are generated within a sphere of radius r using a random distribution
	 **/
	void GenerateAttractorSphere (int n, float r) {
		for (int i = 0; i < n; i++) {
			float radius = Random.Range(0f, 1f);
			radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI/2f), 0.8f);
			radius*= r;
			// 2 angles are generated from which a direction will be computed
			float alpha = Random.Range(0f, Mathf.PI);
			float theta = Random.Range(0f, Mathf.PI*2f);

			Vector3 pt = new Vector3(
				radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
				radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
				radius * Mathf.Cos(alpha)
			);

			// translation to match the parent position
			pt+= transform.position + crownOffset;

            _attractors.Add(pt);
		}
	}

	void GenerateAttractorRing(int n)
	{
        for (int i = 0; i < n; i++)
        {
			float theta = 2f * Mathf.PI * Random.Range(0f, 1f);

            Vector3 pt = new Vector3(
                Mathf.Cos(theta),
                0, //radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                Mathf.Sin(theta)
            );

			//scale vector to fall on ring of previos r to new r
			pt = pt * Random.Range(prevRad, prevRad + _addRadius);

            // translation to match the parent position
            pt += transform.position;

            _attractors.Add(pt);
        }

		prevRad = prevRad + _addRadius;
    }

	
    void GenerateAttractorCircle(int n, float r)
    {
        for (int i = 0; i < n; i++)
        {
            float radius = Random.Range(0f, 1f);
            radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
            radius *= r;
            // 2 angles are generated from which a direction will be computed
            float alpha = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);

            Vector3 pt = new Vector3(
                radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
                0, //radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                radius * Mathf.Cos(alpha)
            );

			// translation to match the parent position
			pt += transform.position;

            _attractors.Add(pt);
        }

		prevRad = r;
    }

    /**
	 * Returns a 3D random vector of _randomGrowth magniture 
	 **/
    Vector3 RandomGrowthVector () {
		float alpha = Random.Range(0f, Mathf.PI);
		float theta = Random.Range(0f, Mathf.PI*2f);

		Vector3 pt = new Vector3(
			Mathf.Cos(theta) * Mathf.Sin(alpha),
			Mathf.Sin(theta) * Mathf.Sin(alpha),
			Mathf.Cos(alpha)
		);

		return pt * _randomGrowth;
	}

	// Start is called before the first frame update
	void Start () {
		//GrowRoots();
  }

	public void GrowRoots()
	{
		cnbAttr = _nbAttractors;
		if (isRoot)
			GenerateAttractorCircle(_nbAttractors, _radius);
		else
		{
			float radSize = Random.Range(2f, 6f);
			Vector3 cOffset = new Vector3(Random.Range(0f, 1.5f), Random.Range(5f, 10f), Random.Range(0f, 1.5f));
			float randGrow = Random.Range(1.2f, 1.7f);

			_radius = radSize;
			crownOffset = cOffset;
			_randomGrowth = randGrow;
            GenerateAttractorSphere(_nbAttractors, _radius);
		}

        _filter = GetComponent<MeshFilter>();

        // we generate the first branch 
        _firstBranch = new Branch(_startPosition, _startPosition + new Vector3(0, _branchLength, 0), new Vector3(0, 1, 0));
        _branches.Add(_firstBranch);
        _extremities.Add(_firstBranch);

		IsGrowing = !IsGrowing;
    }

  // Update is called once per frame
  void Update () {
		if (IsGrowing)
		{
			_timeSinceLastIteration+= Time.deltaTime;

			// we check if we need to run a new iteration 
			if (_timeSinceLastIteration > _timeBetweenIterations) {
				_timeSinceLastIteration = 0f;

				// we parse the extremities to set them as grown 
				foreach (Branch b in _extremities) {
					b._grown = true;
				}

				// we remove the attractors in kill range
				for (int i = _attractors.Count-1; i >= 0; i--) {
					foreach (Branch b in _branches) {
						if (Vector3.Distance(b._end, _attractors[i]) < _killRange) {
							_attractors.Remove(_attractors[i]);
							_nbAttractors--;
							break;
						}
					}
				}

				if (_attractors.Count > 0) {
					// we clear the active attractors
					_activeAttractors.Clear();
					foreach (Branch b in _branches) {
						b._attractors.Clear();
					}

					// each attractor is associated to its closest branch, if in attraction range
					int ia = 0;
					foreach (Vector3 attractor in _attractors) {
						float min = 999999f;
						Branch closest = null; // will store the closest branch
						foreach (Branch b in _branches) {
							float d = Vector3.Distance(b._end, attractor);
							if (d < _attractionRange && d < min) {
								min = d;
								closest = b;
							}
						}

						// if a branch has been found, we add the attractor to the branch
						if (closest != null) {
							closest._attractors.Add(attractor);
							_activeAttractors.Add(ia);
						}

						ia++;
					}

					// if at least an attraction point has been found, we want our tree to grow towards it
					if (_activeAttractors.Count != 0) {
						// because new extremities will be set here, we clear the current ones
						_extremities.Clear();

						// new branches will be added here
						List<Branch> newBranches = new List<Branch>();

						foreach (Branch b in _branches) {
							// if the branch has attraction points, we grow towards them
							if (b._attractors.Count > 0) {
								// we compute the direction of the new branch
								Vector3 dir = new Vector3(0, 0, 0);
								foreach (Vector3 attr in b._attractors) {
									dir+= (attr - b._end).normalized;
								}
								dir/= b._attractors.Count;
								// random growth
								dir+= RandomGrowthVector();
								dir.Normalize();

								// our new branch grows in the correct direction
								Branch nb = new Branch(b._end, b._end + dir * _branchLength, dir, b);
								nb._distanceFromRoot = b._distanceFromRoot+1;
								b._children.Add(nb);
								newBranches.Add(nb);
								_extremities.Add(nb);

								//rarely add child tree
								if ((_childChance / 100) >= Random.Range(0f, 100f))
								{
									GameObject obj = Instantiate(childPrefab, b._end, transform.rotation, transform);

									if (obj.GetComponent<Generator>() != null)
									{
										Generator gen = obj.GetComponent<Generator>();
										childTrees.Add(gen);
										gen.GrowRoots();
									}
									else {
										obj.transform.rotation = Random.rotation;
									}
								}
							} else {
								// if no attraction points, we only check if the branch is an extremity
								if (b._children.Count == 0) {
									_extremities.Add(b);
								}
							}
						}

						// we merge the new branches with the previous ones
						_branches.AddRange(newBranches);
					} else {
						// we grow the extremities of the tree
						for (int i = 0; i < _extremities.Count; i++) {
							Branch e = _extremities[i];
							// the new branch starts where the extremity ends
							Vector3 start = e._end;
							// we add randomness to the direction
							Vector3 dir = e._direction + RandomGrowthVector();
							// we add the direction multiplied by the branch length to get the end point
							Vector3 end = e._end + dir * _branchLength;
							// a new branch can be created with the same direction as its parent
							Branch nb = new Branch(start, end, dir, e);

							// the current extrimity has a new child
							e._children.Add(nb);

							// let's add the new branch to the list and set it as the new extremity 
							_branches.Add(nb);
							_extremities[i] = nb;
						}
					}
				}
			}

			ToMesh();

			if (_attractors.Count < cutoff)
			{
				//IsGrowing = false;
				_activeAttractors.Clear();
				_attractors.Clear();
				//Debug.LogError("hi");
				if (extendedGrowth)
					StartCoroutine(GrowOut());
						//GenerateAttractorRing(cnbAttr);
			}
		}
  }


	/**
	 * Creates a mesh from the branches list
	 **/
	void ToMesh () {
		Mesh treeMesh = new Mesh();

		// we first compute the size of each branch 
		for (int i = _branches.Count-1; i >= 0; i--) {
			float size = 0f;
			Branch b = _branches[i];
			if (b._children.Count == 0) {
				size = _extremitiesSize;
			} else {
				foreach (Branch bc in b._children) {
					size+= Mathf.Pow(bc._size, _invertGrowth);
				}
				size = Mathf.Pow(size, 1f/_invertGrowth);
			}
			b._size = size;
		}

		Vector3[] vertices = new Vector3[(_branches.Count+1) * _radialSubdivisions];
		int[] triangles = new int[_branches.Count * _radialSubdivisions * 6];

		// construction of the vertices 
		for (int i = 0; i < _branches.Count; i++) {
			Branch b = _branches[i];

			// the index position of the vertices
			int vid = _radialSubdivisions*i;
			b._verticesId = vid;

			// quaternion to rotate the vertices along the branch direction
			Quaternion quat = Quaternion.FromToRotation(Vector3.up, b._direction);

			// construction of the vertices 
			for (int s = 0; s < _radialSubdivisions; s++) {
				// radial angle of the vertex
				float alpha = ((float)s/_radialSubdivisions) * Mathf.PI * 2f;

				// radius is hard-coded to 0.1f for now
				Vector3 pos = new Vector3(Mathf.Cos(alpha)* b._size, 0, Mathf.Sin(alpha) * b._size);
				pos = quat * pos; // rotation

				// if the branch is an extremity, we have it growing slowly
				if (b._children.Count == 0 && !b._grown) {
					pos+= b._start + (b._end-b._start) * _timeSinceLastIteration/_timeBetweenIterations;
				} else {
					pos+= b._end;
				}

				vertices[vid+s] = pos - transform.position; // from tree object coordinates to [0; 0; 0]

				// if this is the tree root, vertices of the base are added at the end of the array 
				if (b._parent == null) {
					vertices[_branches.Count*_radialSubdivisions+s] = b._start + new Vector3(Mathf.Cos(alpha)* b._size, 0, Mathf.Sin(alpha)*b._size) - transform.position;
				}
			}
		}

		// faces construction; this is done in another loop because we need the parent vertices to be computed
		for (int i = 0; i < _branches.Count; i++) {
			Branch b = _branches[i];
			int fid = i*_radialSubdivisions*2*3;
			// index of the bottom vertices 
			int bId = b._parent != null ? b._parent._verticesId : _branches.Count*_radialSubdivisions;
			// index of the top vertices 
			int tId = b._verticesId;

			// construction of the faces triangles
			for (int s = 0; s < _radialSubdivisions; s++) {
				// the triangles 
				triangles[fid+s*6] = bId+s;
				triangles[fid+s*6+1] = tId+s;
				if (s == _radialSubdivisions-1) {
					triangles[fid+s*6+2] = tId;
				} else {
					triangles[fid+s*6+2] = tId+s+1;
				}

				if (s == _radialSubdivisions-1) {
					// if last subdivision
					triangles[fid+s*6+3] = bId+s;
					triangles[fid+s*6+4] = tId;
					triangles[fid+s*6+5] = bId;
				} else {
					triangles[fid+s*6+3] = bId+s;
					triangles[fid+s*6+4] = tId+s+1;
					triangles[fid+s*6+5] = bId+s+1;
				}
			}
		}

		treeMesh.vertices = vertices;
		treeMesh.triangles = triangles;
		treeMesh.RecalculateNormals();
		_filter.mesh = treeMesh;
	}

    public void Stop()
    {
		foreach (var tree in childTrees)
		{
			tree.enabled = false;
		}
    }

    /*
	void OnDrawGizmos () {
		/*
		if (_attractors == null) {
			return;
		}
		// we draw the attractors
		for (int i = 0; i < _attractors.Count; i++) {
			if (_activeAttractors.Contains(i)) {
				Gizmos.color = Color.yellow;
			} else {
				Gizmos.color = Color.red;
			}
			Gizmos.DrawSphere(_attractors[i], 0.22f);
		}

		Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
		Gizmos.DrawSphere(_extremities[0]._end, _attractionRange);
		*

		// we draw the branches 
		foreach (Branch b in _branches) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine(b._start, b._end);
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(b._end, 0.05f);
			Gizmos.DrawSphere(b._start, 0.05f);
		}
	}
*/

    private List<Vector3> GenerateSeedRing(int n)
    {
		List<Vector3> tmp = new List<Vector3>();

        for (int i = 0; i < n; i++)
        {
            float theta = 2f * Mathf.PI * Random.Range(0f, 1f);

            Vector3 pt = new Vector3(
                Mathf.Cos(theta),
                0, //radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                Mathf.Sin(theta)
            );

            //scale vector to fall on ring of previos r to new r
            pt = pt * Random.Range(prevRad, prevRad + _addRadius);

            // translation to match the parent position
            pt += transform.position;

			//_attractors.Add(pt);
			tmp.Add(pt);
        }

        prevRad = prevRad + _addRadius;
		return (tmp);
    }

    private IEnumerator GrowOut()
	{
		while (isRoot && IsGrowing)
		{
			yield return new WaitForSeconds(20f);

			List<Vector3> tmp = GenerateSeedRing(Random.Range(0, 7));

            foreach (var seed in tmp)
			{
				Debug.Log("a tree");
                GameObject obj = Instantiate(childPrefab, seed, transform.rotation, transform);

                Generator gen = obj.GetComponent<Generator>();
                childTrees.Add(gen);
                gen.GrowRoots();
                yield return new WaitForSeconds(5f);
			}
		}
    }
}
