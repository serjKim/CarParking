import { dataMember, required } from 'santee-dcts';

export class AppSettings {
    @dataMember()
    @required()
    public apiUrl: string = '';
}
