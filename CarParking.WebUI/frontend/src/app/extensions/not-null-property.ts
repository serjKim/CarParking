export function NotNullProperty() {
    return (target: unknown, propertyKey: string, desc: PropertyDescriptor): PropertyDescriptor => {
        const oldSet = desc.set;
        return {
            configurable: desc.configurable,
            enumerable: desc.enumerable,
            set(val: unknown) {
                if (val == null) {
                    throw new Error(`The '${propertyKey}' can't be null/undefined!`);
                }
                this.value = val;
                if (oldSet) {
                    oldSet.call(this, val);
                }
            },
            get() {
                return this.value;
            },
        };
    };
}
