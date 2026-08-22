SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)

complain() {
    echo "USAGE: ./export_release.sh [linux | windows | all] [ ssh hostname ]"
    exit 1
}

build_linux() {
    godot-mono --path ${SCRIPT_DIR} --export-release "Linux Prod" ${SCRIPT_DIR}/exports/linux_prod/HordeHunters.x86_64 --headless
    (cd ${SCRIPT_DIR}/exports/ && tar -cJvf HordeHunters_linux.tar.xz linux_prod/)
    ARTIFACTS+=(${SCRIPT_DIR}/exports/HordeHunters_linux.tar.xz)
}

build_windows() {
    godot-mono --path ${SCRIPT_DIR}/ --export-release "Windows Prod" ${SCRIPT_DIR}/exports/windows_prod/HordeHunters.exe --headless
    (cd ${SCRIPT_DIR}/exports/ && 7z a -mx9 HordeHunters_windows.7z windows_prod/)
    ARTIFACTS+=(${SCRIPT_DIR}/exports/HordeHunters_windows.7z)
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
