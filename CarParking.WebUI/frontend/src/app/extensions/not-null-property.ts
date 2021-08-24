export function NotNullProperty() {
    return (target: unknown, propertyKey: string, desc: PropertyDescriptor) => {
        const oldSet = desc.set;
        desc.set = function(val: unknown) {
            if (val == null) {
                 throw new Error(`The '${propertyKey}' can't be null/undefined!`);
            }
            desc.value = val;
            if (oldSet) {
                oldSet.call(this, val);
            }
        };
        desc.get = () => {
            return desc.value;
        };
    };
}
