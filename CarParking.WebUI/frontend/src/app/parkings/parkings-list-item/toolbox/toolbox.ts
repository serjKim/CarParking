import { Injectable } from '@angular/core';
import { CompletionResultType, Parking, ParkingType } from '../../models';
import { ParkingsStorage } from '../../parkings.storage';

export enum ToolboxButtonType {
    Complete,
    Pay,
}

export type ToolBoxClick = (parking: Parking, toolbox: Toolbox) => Promise<void>;

export class ToolboxButton {
    public disabled = false;
    public displayed = false;
    constructor(
        public readonly type: ToolboxButtonType,
        public readonly onClick: ToolBoxClick,
    ) {}
}

export class Toolbox {
    constructor(
        public readonly buttons: ToolboxButton[],
    ) {}

    public displayPayButton() {
        for (const button of this.buttons) {
            button.displayed = button.type === ToolboxButtonType.Pay;
        }
    }
}

type ToolboxButtonSettings = { [key in ParkingType]: (completeButton: ToolboxButton, payButton: ToolboxButton) => void };

@Injectable()
export class ToolboxFactory {
    private readonly settingsByType: ToolboxButtonSettings = {
        [ParkingType.StartedFree]: (completeButton: ToolboxButton, payButton: ToolboxButton) => {
            completeButton.displayed = true;
            payButton.displayed = false;
        },
        [ParkingType.CompletedFree]: (completeButton: ToolboxButton, payButton: ToolboxButton) => {
            completeButton.displayed = true;
            completeButton.disabled = true;
            payButton.displayed = false;
        },
        [ParkingType.CompletedFirst]: (completeButton: ToolboxButton, payButton: ToolboxButton) => {
            completeButton.displayed = false;
            payButton.displayed = true;
            payButton.disabled = false;
        },
    };

    constructor(
        private readonly parkingsStorage: ParkingsStorage,
    ) {}

    public createToolbox(type: ParkingType): Toolbox {
        const setupButtons = this.settingsByType[type];
        if (!setupButtons) {
            throw new Error(`Unexpected ${type} parking type.`);
        }

        const completeButton = new ToolboxButton(ToolboxButtonType.Complete, this.onComplete());
        const payButton = new ToolboxButton(ToolboxButtonType.Pay, this.onPay());

        setupButtons(completeButton, payButton);

        return new Toolbox([completeButton, payButton]);
    }

    private onComplete(): ToolBoxClick {
        return async (parking: Parking, toolbox: Toolbox) => {
            if (parking.type === ParkingType.StartedFree) {
                const result = await this.parkingsStorage.completeParking(parking);
                if (result.type === CompletionResultType.FreeExpired) {
                    toolbox.displayPayButton();
                }
            }
        };
    }

    private onPay(): ToolBoxClick {
        return (parking: Parking) => {
            return this.parkingsStorage.payParking(parking);
        };
    }
}
