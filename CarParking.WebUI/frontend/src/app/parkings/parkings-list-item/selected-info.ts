import { ParkingType } from '../models';

export enum InfoType {
    Started,
    Completed,
}

export class SelectedInfo {
    public get type(): InfoType {
        return this.currentType;
    }

    public static started = () => new SelectedInfo(false, InfoType.Started);
    public static completed = () => new SelectedInfo(true, InfoType.Completed);

    private constructor(
        public readonly canSwitch: boolean,
        private currentType: InfoType,
    ) {}

    public trySwitch() {
        if (!this.canSwitch) {
            return;
        }
        this.currentType = this.currentType === InfoType.Started
            ? InfoType.Completed
            : InfoType.Started;
    }
}

export const selectedInfoByType: { [key in ParkingType]: () => SelectedInfo } = {
    [ParkingType.StartedFree]: () => SelectedInfo.started(),
    [ParkingType.CompletedFree]: () => SelectedInfo.completed(),
    [ParkingType.CompletedFirst]: () => SelectedInfo.completed(),
};
