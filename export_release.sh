SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)

complain() {
    echo "USAGE: ./export_release.sh [linux | windows | all] [ ssh hostname ]"
    exit 1
}

build_linux() {
    godot-mono --path ${SCRIPT_DIR} --export-release Linux ${SCRIPT_DIR}/exports/linux/linux.x86_64 --headless
    (cd ${SCRIPT_DIR}/exports/ && tar -cJvf linux.tar.xz linux/)
    ARTIFACTS+=(${SCRIPT_DIR}/exports/linux.tar.xz)
}

build_windows() {
    godot-mono --path ${SCRIPT_DIR}/ --export-release Windows ${SCRIPT_DIR}/exports/windows/windows.exe --headless
    (cd ${SCRIPT_DIR}/exports/ && 7z a -mx9 windows.7z windows/)
    ARTIFACTS+=(${SCRIPT_DIR}/exports/windows.7z)
}

ARTIFACTS=()

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

if [ -n "$2" ]; then
    if [ ${#ARTIFACTS[@]} -eq 0 ]; then
        echo "No artifacts to send"
        exit 0
    fi
    ssh "$2" mkdir -p Downloads
    scp "${ARTIFACTS[@]}" "$2:Downloads/"
fi
