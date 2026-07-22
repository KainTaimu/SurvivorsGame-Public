SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)

complain() {
    echo "USAGE: ./export_release.sh [linux | windows | all] [ ssh hostname ]"
    exit 1
}

build_linux() {
    godot-mono --path ${SCRIPT_DIR} --export-release Linux ${SCRIPT_DIR}/exports/linux/linux.x86_64 --headless
    (cd ${SCRIPT_DIR}/exports/ && tar -cJvf linux.tar.xz linux/)
}

build_windows() {
    godot-mono --path ${SCRIPT_DIR}/ --export-release Windows ${SCRIPT_DIR}/exports/windows/windows.exe --headless
    (cd ${SCRIPT_DIR}/exports/ && zip -r -9 windows.zip windows/)
}

case $1 in
linux)
    build_linux
    ;;
windows)
    build_windows
    ;;
all)
    build_linux
    build_windows
    ;;
_)
    ;;
*)
    complain
    ;;
esac
