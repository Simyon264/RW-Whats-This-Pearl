import os
import shutil

common_steam_locations = [
    "C:/Program Files (x86)/Steam/steamapps/common/Rain World",
    "C:/Program Files/Steam/steamapps/common/Rain World"
]

files_to_copy = [ # https://rainworldmodding.miraheze.org/wiki/Code_Environments#Common_references
    "BepInEx/utils/PUBLIC-Assembly-CSharp.dll",
    "RainWorld_Data/Managed/UnityEngine.dll",
    "RainWorld_Data/Managed/UnityEngine.CoreModule.dll",
    "RainWorld_Data/Managed/UnityEngine.InputLegacyModule.dll",
    "BepInEx/core/BepInEx.dll",
    "BepInEx/plugins/HOOKS-Assembly-CSharp.dll",
    "BepInEx/core/MonoMod.RuntimeDetour.dll",
    "BepInEx/core/MonoMod.Utils.dll"
]

dest = "." # Current directory

def get_rain_world_root() -> str:
    for location in common_steam_locations:
        if os.path.exists(location):
            return location

    manual = input("Could not find Rain World installation. Please enter the path manually: ")
    if os.path.exists(manual):
        return manual
    else:
        print("Invalid path.")
        return get_rain_world_root()


def get_files():
    root = get_rain_world_root()
    for file in files_to_copy:
        path = os.path.join(root, file)
        if os.path.exists(path):
            shutil.copy(path, dest)
            print(f"Copied {file}")
        else:
            raise FileNotFoundError(f"File not found: {path}")

    print("Files copied successfully.")

def have_files_already() -> bool:
    files_in_dir = os.listdir(dest)
    for file in files_in_dir:
        if file.endswith(".dll"):
            return True

    return False


def delete_all_dlls():
    files_in_dir = os.listdir(dest)
    for file in files_in_dir:
        if file.endswith(".dll"):
            os.remove(os.path.join(dest, file))
            print(f"Deleted {file}")

    print("Deleted all DLLs.")

if __name__ == "__main__":
    if have_files_already():
        print("Files already exist in the current directory.")
        if input("Would you like to clear them? (y/n): ").lower() == "n":
            exit()
        else:
            delete_all_dlls()

    get_files()