using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ToJ
{
	[ExecuteInEditMode]
	[AddComponentMenu("Alpha Mask")]
	public class Mask : MonoBehaviour
	{
		public enum MappingAxis
		{
			X,
			Y,
			Z
		};

		[SerializeField]
		private MappingAxis _maskMappingWorldAxis = MappingAxis.Z;
		public MappingAxis maskMappingWorldAxis
		{
			get
			{
				return _maskMappingWorldAxis;
			}
			set
			{
				ChangeMappingAxis(value, _maskMappingWorldAxis, _invertAxis);
				_maskMappingWorldAxis = value;
			}
		}

		[SerializeField]
		private bool _invertAxis = false;
		public bool invertAxis
		{
			get
			{
				return _invertAxis;
			}
			set
			{
				ChangeMappingAxis(_maskMappingWorldAxis, _maskMappingWorldAxis, value);
				_invertAxis = value;
			}
		}

		[SerializeField]
		private bool _clampAlphaHorizontally = false;
		public bool clampAlphaHorizontally
		{
			get
			{
				return _clampAlphaHorizontally;
			}
			set
			{
				SetMaskBoolValueInMaterials("_ClampHoriz", value);
				_clampAlphaHorizontally = value;
			}
		}

		[SerializeField]
		private bool _clampAlphaVertically = false;
		public bool clampAlphaVertically
		{
			get
			{
				return _clampAlphaVertically;
			}
			set
			{
				SetMaskBoolValueInMaterials("_ClampVert", value);
				_clampAlphaVertically = value;
			}
		}

		[SerializeField]
		private float _clampingBorder = 0.01f;
		public float clampingBorder
		{
			get
			{
				return _clampingBorder;
			}
			set
			{
				SetMaskFloatValueInMaterials("_ClampBorder", value);
				_clampingBorder = value;
			}
		}

		[SerializeField]
		private bool _useMaskAlphaChannel = false;
		public bool useMaskAlphaChannel
		{
			get
			{
				return _useMaskAlphaChannel;
			}
			set
			{
				SetMaskBoolValueInMaterials("_UseAlphaChannel", value);
				_useMaskAlphaChannel = value;
			}
		}


		private Shader _maskedSpriteWorldCoordsShader;
		private Shader _maskedUnlitWorldCoordsShader;


		void Start()
		{
			_maskedSpriteWorldCoordsShader = Shader.Find("Alpha Masked/Sprites Alpha Masked - World Coords");
			_maskedUnlitWorldCoordsShader = Shader.Find("Alpha Masked/Unlit Alpha Masked - World Coords");

			MeshRenderer maskMeshRenderer = GetComponent<MeshRenderer>();
			MeshFilter maskMeshFilter = GetComponent<MeshFilter>();
			if (Application.isPlaying)
			{
				if (maskMeshRenderer != null)
				{
					maskMeshRenderer.enabled = false;
				}
			}
			#if UNITY_EDITOR
			else
			{
				if (maskMeshFilter == null)
				{
					maskMeshFilter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
					maskMeshFilter.sharedMesh = new Mesh();
					CreateAndAssignQuad(maskMeshFilter.sharedMesh);
					maskMeshFilter.sharedMesh.name = "Mask Quad";
				}
				if (maskMeshRenderer == null)
				{
					maskMeshRenderer = gameObject.AddComponent<MeshRenderer>() as MeshRenderer;
					maskMeshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
					maskMeshRenderer.sharedMaterial.name = "Mask Material";
				}

				maskMappingWorldAxis = _maskMappingWorldAxis;
				invertAxis = _invertAxis;
			}
			#endif

		}


		void Update()
		{
			if (_maskedSpriteWorldCoordsShader == null)
			{
				_maskedSpriteWorldCoordsShader = Shader.Find("Alpha Masked/Sprites Alpha Masked - World Coords");
			}
			if (_maskedUnlitWorldCoordsShader == null)
			{
				_maskedUnlitWorldCoordsShader = Shader.Find("Alpha Masked/Unlit Alpha Masked - World Coords");
			}

			if ((_maskedSpriteWorldCoordsShader == null) || (_maskedUnlitWorldCoordsShader == null))
			{
				Debug.Log("Shaders necessary for masking don't seem to be present in the project.");
				return;
			}

			if (transform.hasChanged == true)
			{
				if ((maskMappingWorldAxis == MappingAxis.X) && ((Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, 0)) > 0.01f) || (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, invertAxis ? -90 : 90)) > 0.01f)))
				{
					Debug.Log("You cannot edit X and Y values of the Mask transform rotation!");
					transform.eulerAngles = new Vector3(0, invertAxis ? 270 : 90, transform.eulerAngles.z);
				}
				else if ((maskMappingWorldAxis == MappingAxis.Y) && ((Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, invertAxis ? -90 : 90)) > 0.01f) || (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0)) > 0.01f)))
				{
					Debug.Log("You cannot edit X and Z values of the Mask transform rotation!");
					transform.eulerAngles = new Vector3(invertAxis ? -90 : 90, transform.eulerAngles.y, 0);
				}
				else if ((maskMappingWorldAxis == MappingAxis.Z) && ((Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, 0)) > 0.01f) || (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, (invertAxis ? -180 : 0))) > 0.01f)))
				{
					Debug.Log("You cannot edit X and Y values of the Mask transform rotation!");
					transform.eulerAngles = new Vector3(0, invertAxis ? -180 : 0, transform.eulerAngles.z);
				}

				if (transform.parent != null)
				{
					Renderer[] renderers = transform.parent.gameObject.GetComponentsInChildren<Renderer>();
					Graphic[] UIComponents = transform.parent.gameObject.GetComponentsInChildren<Graphic>();
					List<Material> differentMaterials = new List<Material>();

					// Needed for UI Screen Space - Overlay support
					Dictionary<Material, Graphic> UIMaterialToGraphic = new Dictionary<Material, Graphic>();

					foreach (Renderer renderer in renderers)
					{
						if (renderer.gameObject != gameObject)
						{
							foreach (Material material in renderer.sharedMaterials)
							{
								if (!differentMaterials.Contains(material))
								{
									differentMaterials.Add(material);
								}
							}
						}
					}

					foreach (Graphic UIComponent in UIComponents)
					{
						if (UIComponent.gameObject != gameObject)
						{
							if (!differentMaterials.Contains(UIComponent.material))
							{
								differentMaterials.Add(UIComponent.material);

								// Needed for UI "Screen Space - Overlay" support
								Canvas currCanvas = UIComponent.canvas;
								if ((currCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ||
									(currCanvas.renderMode == RenderMode.ScreenSpaceCamera) && (currCanvas.worldCamera == null))
								{
									UIMaterialToGraphic.Add(differentMaterials[differentMaterials.Count - 1], UIComponent);
								}
							}
						}
					}

					foreach (Material material in differentMaterials)
					{
						if ((material.shader.ToString() == _maskedSpriteWorldCoordsShader.ToString()) &&
							(material.shader.GetInstanceID() != _maskedSpriteWorldCoordsShader.GetInstanceID()))
						{
							Debug.Log("There seems to be more than one masked shader in the project with the same display name, and it's preventing the mask from being properly applied.");
							_maskedSpriteWorldCoordsShader = null;
						}
						if ((material.shader.ToString() == _maskedUnlitWorldCoordsShader.ToString()) &&
							(material.shader.GetInstanceID() != _maskedUnlitWorldCoordsShader.GetInstanceID()))
						{
							Debug.Log("There seems to be more than one masked shader in the project with the same display name, and it's preventing the mask from being properly applied.");
							_maskedUnlitWorldCoordsShader = null;
						}


						if ((material.shader == _maskedSpriteWorldCoordsShader) ||
							(material.shader == _maskedUnlitWorldCoordsShader))
						{
							material.DisableKeyword("_SCREEN_SPACE_UI");

							Vector2 scale = new Vector2(1f / transform.lossyScale.x, 1f / transform.lossyScale.y);
							Vector2 offset = Vector2.zero;
							float rotationAngle = 0;
							int sign = 1;
							if (maskMappingWorldAxis == MappingAxis.X)
							{
								sign = (invertAxis ? 1 : -1);
								offset = new Vector2(-transform.position.z, -transform.position.y);
								rotationAngle = sign * transform.eulerAngles.z;
							}
							else if (maskMappingWorldAxis == MappingAxis.Y)
							{
								offset = new Vector2(-transform.position.x, -transform.position.z);
								rotationAngle = -transform.eulerAngles.y;
							}
							else if (maskMappingWorldAxis == MappingAxis.Z)
							{
								sign = (invertAxis ? -1 : 1);
								offset = new Vector2(-transform.position.x, -transform.position.y);
								rotationAngle = sign * transform.eulerAngles.z;
							}

							// Needed for UI anchor support
							RectTransform maskRectTransform = GetComponent<RectTransform>();
							if (maskRectTransform != null)
							{
								Rect rect = maskRectTransform.rect;
							
								offset += (Vector2)(transform.right * (maskRectTransform.pivot.x - 0.5f) * rect.width * transform.lossyScale.x +
									transform.up * (maskRectTransform.pivot.y - 0.5f) * rect.height * transform.lossyScale.y);
								scale.x /= rect.width;
								scale.y /= rect.height;

								#if UNITY_EDITOR
								if (!Application.isPlaying)
								{
									MeshFilter maskMeshFilter = GetComponent<MeshFilter>();
									if (maskMeshFilter != null)
									{
										CreateAndAssignQuad(maskMeshFilter.sharedMesh, rect.xMin, rect.xMax, rect.yMin, rect.yMax);
									}
								}
								#endif
							}
							#if UNITY_EDITOR
							else
							{
								if (!Application.isPlaying)
								{
									MeshFilter maskMeshFilter = GetComponent<MeshFilter>();
									if (maskMeshFilter != null)
									{
										CreateAndAssignQuad(maskMeshFilter.sharedMesh);
									}
								}
							}
							#endif

							// UI "Screen Space - Overlay mode" or "Screen Space - Camera", where the camera is null (actually, falls back to "Overlay")
							if (UIMaterialToGraphic.ContainsKey(material))
							{
								offset = UIMaterialToGraphic[material].transform.InverseTransformVector(offset);

								switch (maskMappingWorldAxis)
								{
									case MappingAxis.X:
										offset.x *= UIMaterialToGraphic[material].transform.lossyScale.z;
										offset.y *= UIMaterialToGraphic[material].transform.lossyScale.y;
										break;
									case MappingAxis.Y:
										offset.x *= UIMaterialToGraphic[material].transform.lossyScale.x;
										offset.y *= UIMaterialToGraphic[material].transform.lossyScale.z;
										break;
									case MappingAxis.Z:
										offset.x *= UIMaterialToGraphic[material].transform.lossyScale.x;
										offset.y *= UIMaterialToGraphic[material].transform.lossyScale.y;
										break;
								}
								Canvas currUIElementCanvas = UIMaterialToGraphic[material].canvas;
								offset /= currUIElementCanvas.scaleFactor;

								offset = RotateVector(offset, UIMaterialToGraphic[material].transform.eulerAngles);
								offset += currUIElementCanvas.GetComponent<RectTransform>().sizeDelta * 0.5f;
								scale *= currUIElementCanvas.scaleFactor;

								material.EnableKeyword("_SCREEN_SPACE_UI");
							}


							Vector2 scaleTexture = gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureScale;
							scale.x *= scaleTexture.x;
							scale.y *= scaleTexture.y;

							scale.x *= sign;

							Vector2 offsetTemporary = offset;
							float s = Mathf.Sin(-rotationAngle * Mathf.Deg2Rad);
							float c = Mathf.Cos(-rotationAngle * Mathf.Deg2Rad);

							offset.x = (c * offsetTemporary.x - s * offsetTemporary.y) * scale.x + 0.5f * scaleTexture.x;
							offset.y = (s * offsetTemporary.x + c * offsetTemporary.y) * scale.y + 0.5f * scaleTexture.y;


							offset += gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureOffset;

							material.SetTextureOffset("_AlphaTex", offset);
							material.SetTextureScale("_AlphaTex", scale);
							material.SetFloat("_MaskRotation", rotationAngle * Mathf.Deg2Rad);
						}
					}
				}
			}
		}


		private Vector3 RotateVector (Vector3 point, Vector3 angles)
		{
			return Quaternion.Euler(angles) * point;
		}

		private void SetMaskMappingAxisInMaterials(MappingAxis mappingAxis)
		{
			if (transform.parent == null)
			{
				return;
			}

			Renderer[] renderers = transform.parent.gameObject.GetComponentsInChildren<Renderer>();
			Graphic[] UIComponents = transform.parent.gameObject.GetComponentsInChildren<Graphic>();
			List<Material> differentMaterials = new List<Material>();

			foreach (Renderer renderer in renderers)
			{
				if (renderer.gameObject != gameObject)
				{
					foreach (Material material in renderer.sharedMaterials)
					{
						if (!differentMaterials.Contains(material))
						{
							differentMaterials.Add(material);

							SetMaskMappingAxisInMaterial(mappingAxis, material);
						}
					}
				}
			}

			foreach (Graphic UIComponent in UIComponents)
			{
				if (UIComponent.gameObject != gameObject)
				{
					if (!differentMaterials.Contains(UIComponent.material))
					{
						differentMaterials.Add(UIComponent.material);

						SetMaskMappingAxisInMaterial(mappingAxis, UIComponent.material);
					}
				}
			}
		}

		public void SetMaskMappingAxisInMaterial(MappingAxis mappingAxis, Material material)
		{
			if ((material.shader == _maskedSpriteWorldCoordsShader) ||
				(material.shader == _maskedUnlitWorldCoordsShader))
			{
				switch (mappingAxis)
				{
					case MappingAxis.X:
						material.SetFloat("_Axis", 0);
						material.EnableKeyword("_AXIS_X");
						material.DisableKeyword("_AXIS_Y");
						material.DisableKeyword("_AXIS_Z");
						break;
					case MappingAxis.Y:
						material.SetFloat("_Axis", 1);
						material.DisableKeyword("_AXIS_X");
						material.EnableKeyword("_AXIS_Y");
						material.DisableKeyword("_AXIS_Z");
						break;
					case MappingAxis.Z:
						material.SetFloat("_Axis", 2);
						material.DisableKeyword("_AXIS_X");
						material.DisableKeyword("_AXIS_Y");
						material.EnableKeyword("_AXIS_Z");
						break;
				}
			}
		}

		private void SetMaskFloatValueInMaterials(string variable, float value)
		{
			if (transform.parent == null)
			{
				return;
			}

			Renderer[] renderers = transform.parent.gameObject.GetComponentsInChildren<Renderer>();
			Graphic[] UIComponents = transform.parent.gameObject.GetComponentsInChildren<Graphic>();
			List<Material> differentMaterials = new List<Material>();

			foreach (Renderer renderer in renderers)
			{
				if (renderer.gameObject != gameObject)
				{
					foreach (Material material in renderer.sharedMaterials)
					{
						if (!differentMaterials.Contains(material))
						{
							differentMaterials.Add(material);

							material.SetFloat(variable, value);
						}
					}
				}
			}

			foreach (Graphic UIComponent in UIComponents)
			{
				if (UIComponent.gameObject != gameObject)
				{
					if (!differentMaterials.Contains(UIComponent.material))
					{
						differentMaterials.Add(UIComponent.material);

						UIComponent.material.SetFloat(variable, value);
					}
				}
			}
		}

		private void SetMaskBoolValueInMaterials(string variable, bool value)
		{
			if (transform.parent == null)
			{
				return;
			}

			Renderer[] renderers = transform.parent.gameObject.GetComponentsInChildren<Renderer>();
			Graphic[] UIComponents = transform.parent.gameObject.GetComponentsInChildren<Graphic>();
			List<Material> differentMaterials = new List<Material>();

			foreach (Renderer renderer in renderers)
			{
				if (renderer.gameObject != gameObject)
				{
					foreach (Material material in renderer.sharedMaterials)
					{
						if (!differentMaterials.Contains(material))
						{
							differentMaterials.Add(material);

							SetMaskBoolValueInMaterial(variable, value, material);
						}
					}
				}
			}

			foreach (Graphic UIComponent in UIComponents)
			{
				if (UIComponent.gameObject != gameObject)
				{
					if (!differentMaterials.Contains(UIComponent.material))
					{
						differentMaterials.Add(UIComponent.material);

						SetMaskBoolValueInMaterial(variable, value, UIComponent.material);
					}
				}
			}
		}

		public void SetMaskBoolValueInMaterial(string variable, bool value, Material material)
		{
			if ((material.shader == _maskedSpriteWorldCoordsShader) ||
				(material.shader == _maskedUnlitWorldCoordsShader))
			{
				material.SetFloat(variable, (value ? 1 : 0));
				if (value == true)
				{
					material.EnableKeyword(variable.ToUpper() + "_ON");
				}
				else
				{
					material.DisableKeyword(variable.ToUpper() + "_ON");
				}
			}
		}

		private void CreateAndAssignQuad(Mesh mesh, float minX = -0.5f, float maxX = 0.5f, float minY = -0.5f, float maxY = 0.5f)
		{
			// assign vertices
			Vector3[] vertices = new Vector3[4];

			vertices[0] = new Vector3(minX, minY, 0);
			vertices[1] = new Vector3(maxX, minY, 0);
			vertices[2] = new Vector3(minX, maxY, 0);
			vertices[3] = new Vector3(maxX, maxY, 0);

			mesh.vertices = vertices;

			// assign triangles
			int[] tri = new int[6];

			//  Lower left triangle.
			tri[0] = 0;
			tri[1] = 2;
			tri[2] = 1;

			//  Upper right triangle.   
			tri[3] = 2;
			tri[4] = 3;
			tri[5] = 1;

			mesh.triangles = tri;

			// assign normals
			Vector3[] normals = new Vector3[4];

			normals[0] = -Vector3.forward;
			normals[1] = -Vector3.forward;
			normals[2] = -Vector3.forward;
			normals[3] = -Vector3.forward;

			mesh.normals = normals;

			// assign UVs
			Vector2[] uv = new Vector2[4];

			uv[0] = new Vector2(0, 0);
			uv[1] = new Vector2(1, 0);
			uv[2] = new Vector2(0, 1);
			uv[3] = new Vector2(1, 1);

			mesh.uv = uv;
		}

		public void SetMaskRendererActive(bool value)
		{
			if (GetComponent<Renderer>() != null)
			{
				if (value == true)
				{
					GetComponent<Renderer>().enabled = true;
				}
				else
				{
					GetComponent<Renderer>().enabled = false;
				}
			}
		}
	
		private void ChangeMappingAxis(MappingAxis currMaskMappingWorldAxis, MappingAxis prevMaskMappingWorldAxis, bool currInvertAxis)
		{
			if (currMaskMappingWorldAxis == MappingAxis.X)
			{
				if (prevMaskMappingWorldAxis == MappingAxis.Y)
				{
					transform.eulerAngles = new Vector3(0, currInvertAxis ? -90 : 90, transform.eulerAngles.y);
				}
				else
				{
					transform.eulerAngles = new Vector3(0, currInvertAxis ? -90 : 90, transform.eulerAngles.z);
				}
			}
			else if (currMaskMappingWorldAxis == MappingAxis.Y)
			{
				if (prevMaskMappingWorldAxis == MappingAxis.Y)
				{
					transform.eulerAngles = new Vector3(currInvertAxis ? -90 : 90, transform.eulerAngles.y, 0);
				}
				else
				{
					transform.eulerAngles = new Vector3(currInvertAxis ? -90 : 90, transform.eulerAngles.z, 0);
				}
			}
			else if (currMaskMappingWorldAxis == MappingAxis.Z)
			{
				if (prevMaskMappingWorldAxis == MappingAxis.Y)
				{
					transform.eulerAngles = new Vector3(0, currInvertAxis ? -180 : 0, transform.eulerAngles.y);
				}
				else
				{
					transform.eulerAngles = new Vector3(0, currInvertAxis ? -180 : 0, transform.eulerAngles.z);
				}
			}

			SetMaskMappingAxisInMaterials(currMaskMappingWorldAxis);
		}
	}
}