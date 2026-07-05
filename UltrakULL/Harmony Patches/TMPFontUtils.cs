using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UltrakULL.Harmony_Patches
{
	public static class TMPFontUtils
	{

		private static bool HasProperty(Material material, string propertyName)
		{
			return material != null && material.HasProperty(propertyName);
		}

		private static Material CreateMigratedMaterial(Material sourceMaterial, Material targetMaterial)
		{
			if (targetMaterial == null)
				return sourceMaterial != null ? new Material(sourceMaterial) : null;

			if (sourceMaterial == null)
				return new Material(targetMaterial);

			try
			{
				Material fallbackMaterial = TMP_MaterialManager.GetFallbackMaterial(sourceMaterial, targetMaterial);
				if (fallbackMaterial != null)
					return new Material(fallbackMaterial);
			}
			catch { }

			Material migratedMaterial = new Material(targetMaterial);
			try
			{
				TMP_MaterialManager.CopyMaterialPresetProperties(sourceMaterial, migratedMaterial);
			}
			catch
			{
				foreach (string propertyName in sourceMaterial.GetTexturePropertyNames())
				{
					if (HasProperty(migratedMaterial, propertyName))
						migratedMaterial.SetTexture(propertyName, sourceMaterial.GetTexture(propertyName));
				}
			}
			return migratedMaterial;
		}

		public static void ApplyUnderlayAndZTest(
			TextMeshProUGUI instance,
			Vector4 underlayColor,
			Vector4 underlayOffset,
			float underlaySoftness,
			float underlayDilate,
			bool preserveExistingUnderlay,
			bool isUnderlaid,
			bool isOverlay,
			bool editOverlayStatus,
			TMP_FontAsset fontAsset,
			Material overlayMat,
			Material normalMat)
		{
			if (instance == null || fontAsset == null)
				return;

			string instanceName = instance.gameObject.name;
			Material previousInstanceMaterial = ((TMP_Text)instance).fontMaterial;
			Material previousSharedMaterial = ((TMP_Text)instance).fontSharedMaterial;
			Material currentMaterial = previousInstanceMaterial ?? previousSharedMaterial;

			Material baseMaterial = editOverlayStatus ? (isOverlay ? overlayMat : normalMat) : (((TMP_Asset)fontAsset)?.material ?? normalMat);
			if (baseMaterial == null && currentMaterial == null)
				return;

            bool needsUniqueMaterial = isUnderlaid || editOverlayStatus;

            if (!needsUniqueMaterial)
            {
                ((TMP_Text)instance).font = fontAsset;
                ((TMP_Text)instance).fontSharedMaterial = fontAsset.material;
                return;
            }

            bool sourceHadUnderlay = currentMaterial != null && currentMaterial.IsKeywordEnabled("UNDERLAY_ON");
			bool shouldKeepUnderlay = isUnderlaid || (preserveExistingUnderlay && sourceHadUnderlay);

			//Logging.Message($"[TMPFU] ApplyUnderlay: {instanceName}, isUnderlaid={isUnderlaid}, preserveExisting={preserveExistingUnderlay}, sourceHadUnderlay={sourceHadUnderlay}");
			//Logging.Message($"[TMPFU]   currentMat={currentMaterial?.name ?? "NULL"}, baseMat={baseMaterial?.name ?? "NULL"}");
			//Logging.Message($"[TMPFU]   underlayColor=({underlayColor.x:F2},{underlayColor.y:F2},{underlayColor.z:F2},{underlayColor.w:F2}), offset=({underlayOffset.x:F2},{underlayOffset.y:F2}), softness={underlaySoftness}, dilate={underlayDilate}");
			//Logging.Message($"[TMPFU]   source.shader={currentMaterial?.shader?.name ?? "NULL"}, keyword={currentMaterial?.IsKeywordEnabled("UNDERLAY_ON")}");

			((TMP_Text)instance).font = fontAsset;

			Material val = CreateMigratedMaterial(currentMaterial, baseMaterial);
			if (val == null)
				val = new Material(baseMaterial ?? currentMaterial);


			bool supportsUnderlay = HasProperty(val, "_UnderlayOffset");
			//Logging.Message($"[TMPFU]   val: shader={val.shader?.name}, keyword={val.IsKeywordEnabled("UNDERLAY_ON")}, supportsUnderlay={supportsUnderlay}");

			if (supportsUnderlay && shouldKeepUnderlay)
			{
				//Logging.Message($"[TMPFU]   Setting underlay properties");
				if (HasProperty(val, "_UnderlayColor"))
					val.SetVector("_UnderlayColor", underlayColor);
				if (HasProperty(val, "_UnderlayOffset"))
					val.SetVector("_UnderlayOffset", underlayOffset);
				if (HasProperty(val, "_UnderlaySoftness"))
					val.SetFloat("_UnderlaySoftness", underlaySoftness);
				if (HasProperty(val, "_UnderlayDilate"))
					val.SetFloat("_UnderlayDilate", underlayDilate);
			}
			else if (supportsUnderlay)
			{
				//Logging.Message($"[TMPFU]   Clearing underlay");
				if (HasProperty(val, "_UnderlayColor"))
					val.SetVector("_UnderlayColor", new Vector4(0f, 0f, 0f, 0f));
				if (HasProperty(val, "_UnderlayOffset"))
					val.SetVector("_UnderlayOffset", Vector4.zero);
				if (HasProperty(val, "_UnderlaySoftness"))
					val.SetFloat("_UnderlaySoftness", 0f);
				if (HasProperty(val, "_UnderlayDilate"))
					val.SetFloat("_UnderlayDilate", 0f);
			}
			else
			{
				//Logging.Message($"[TMPFU]   Skipping underlay (Mobile shader)");
			}

			if (editOverlayStatus && HasProperty(val, "_ZTest"))
				val.SetFloat("_ZTest", isOverlay ? 8f : 4f);

			//Logging.Message($"[TMPFU]   Final: keyword={val.IsKeywordEnabled("UNDERLAY_ON")}, shader={val.shader?.name ?? "NULL"}");
			if (HasProperty(val, "_UnderlayColor"))
			{
				string offsetStr = HasProperty(val, "_UnderlayOffset")
					? $"({val.GetVector("_UnderlayOffset").x:F2},{val.GetVector("_UnderlayOffset").y:F2})"
					: "N/A";
				//Logging.Message($"[TMPFU]   _UnderlayColor=({val.GetVector("_UnderlayColor").x:F2},{val.GetVector("_UnderlayColor").y:F2},{val.GetVector("_UnderlayColor").z:F2},{val.GetVector("_UnderlayColor").w:F2}), offset={offsetStr}");
			}
			if (HasProperty(val, "_UnderlaySoftness"))
				//Logging.Message($"[TMPFU]   _UnderlaySoftness={val.GetFloat("_UnderlaySoftness")}, _UnderlayDilate={val.GetFloat("_UnderlayDilate")}");

			((TMP_Text)instance).fontMaterial = val;
			//Logging.Message($"[TMPFU]   fontMaterial assigned");

			//Logging.Message($"[TMPFU] Done: {instanceName}");
		}
	}
}
