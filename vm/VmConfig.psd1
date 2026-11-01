@{
    # The isolated VMware guest used exclusively for Windows TSF experiments.
    VmPath = 'E:\_\VM\NganImeWin10\NganImeWin10.vmx'

    # VMware Workstation is already installed on the host at this location.
    VmrunPath = 'D:\SOFT\VMware\VMware_Workstation17\vmrun.exe'

    # All automation-owned files in the guest remain below this directory.
    GuestRoot = 'C:\NganImeVm'

    # The source scope is deliberately limited to the standalone proof of concept.
    SourcePath = 'E:\_code\Ngan\AvlnImeDemo'

    # Host-side collection location. This script never deletes prior collections.
    LogRoot = 'E:\_code\Ngan\AvlnImeDemo\vm\artifacts\logs'
}
