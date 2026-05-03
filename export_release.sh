SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)

godot-mono --path ${SCRIPT_DIR} --export-release Linux ${SCRIPT_DIR}/exports/linux/linux.x86_64 --headless
(cd ${SCRIPT_DIR}/exports/ && tar -cJvf linux.tar.xz linux/)
godot-mono --path ${SCRIPT_DIR}/ --export-release Windows ${SCRIPT_DIR}/exports/windows/windows.exe --headless
(cd ${SCRIPT_DIR}/exports/ && zip -r -9 windows.zip windows/)
