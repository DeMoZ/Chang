"""Copy rendered PNGs from png/<Category>/ into the Unity project, create .meta files
for new files/folders and register new category folders in the Addressables image group."""
import os
import re
import shutil
import sys
import uuid

ROOT = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.dirname(os.path.dirname(ROOT))
IMAGES = os.path.join(PROJECT, "Assets/Project/Resources_Bundled/ImageWords/Thai")
TEMPLATE_META = os.path.join(IMAGES, "Fruits/Coconut.png.meta")
GROUP = os.path.join(PROJECT, "Assets/AddressableAssetsData/AssetGroups/Remote_Thai_Image_Words.asset")

FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

GROUP_ENTRY = """  - m_GUID: {guid}
    m_Address: Assets/Project/Resources_Bundled/ImageWords/Thai/{category}
    m_ReadOnly: 0
    m_SerializedLabels: []
    FlaggedDuringContentUpdateRestriction: 0
"""


def sprite_meta_template():
    text = open(TEMPLATE_META).read()
    text = text.replace("labels:\n- UnityAI\n", "")
    # single sprite mode: drop the generated sub-sprite, Unity recreates the main sprite itself
    text = re.sub(r"  internalIDToNameTable:\n(?:  - .*\n(?:    .*\n)*)+", "  internalIDToNameTable: []\n", text)
    text = re.sub(r"    sprites:\n(?:    - .*\n(?:      .*\n)*)+", "    sprites: []\n", text)
    text = re.sub(r"    nameFileIdTable:\n(?:      .*\n)+", "    nameFileIdTable: {}\n", text)
    return text


def main(categories):
    template = sprite_meta_template()
    group = open(GROUP).read()
    added_files = replaced_files = 0
    for category in categories:
        src = os.path.join(ROOT, "png", category)
        dst = os.path.join(IMAGES, category)
        if not os.path.isdir(dst):
            os.makedirs(dst)
            folder_guid = uuid.uuid4().hex
            with open(dst + ".meta", "w") as f:
                f.write(FOLDER_META.format(guid=folder_guid))
        else:
            folder_guid = re.search(r"guid: (\w+)", open(dst + ".meta").read()).group(1)
        if folder_guid not in group:
            group = group.replace("  m_ReadOnly: 0\n  m_Settings:",
                                  GROUP_ENTRY.format(guid=folder_guid, category=category) + "  m_ReadOnly: 0\n  m_Settings:", 1)
        for name in sorted(os.listdir(src)):
            if not name.endswith(".png") or name == "_contact.png":
                continue
            target = os.path.join(dst, name)
            existed = os.path.exists(target)
            shutil.copyfile(os.path.join(src, name), target)
            if os.path.exists(target + ".meta"):
                replaced_files += existed
            else:
                with open(target + ".meta", "w") as f:
                    f.write(re.sub(r"guid: \w+", "guid: " + uuid.uuid4().hex, template, count=1))
                added_files += 1
    with open(GROUP, "w") as f:
        f.write(group)
    print(f"added {added_files}, replaced {replaced_files}")


if __name__ == "__main__":
    main(sys.argv[1:])
