
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)

complain() {
    echo "USAGE: ./export_release.sh [linux | windows | all] [ ssh hostname ]"
    exit 1
}

build_linux() {
    godot-mono --path ${SCRIPT_DIR} --export-release Linux ${SCRIPT_DIR}/exports/linux/linux.x86_64 --headless
}

build_windows() {
    godot-mono --path ${SCRIPT_DIR}/ --export-release Windows ${SCRIPT_DIR}/exports/windows/windows.exe --headless
}

ARTIFACTS=()

case $1 in
linux)
    build_linux
    gamemoderun ./exports/linux/linux.x86_64
    ;;
windows)
    build_windows
    wine ./exports/windows/windows.exe
    ;;
_)
    ;;
*)
    complain
    ;;
esac